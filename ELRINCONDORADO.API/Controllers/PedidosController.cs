using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

// Módulo Pedidos y ventas. Solo lectura.
//
// Decisiones que conviene conocer antes de consumir esto:
//
// 1. No existe tabla 'metodos_pago'. 'estado_pago' es un varchar libre; los
//    valores que hay hoy son EFECTIVO, QR y TARJETA. Si el POS empieza a guardar
//    otros, la API los devuelve sin problema pero no los puede validar.
//
// 2. 'fecha_creacion' guarda hora LOCAL del restaurante en un timestamp sin zona.
//    No se convierte a UTC, porque hacerlo correría los pedidos de la tarde al día
//    siguiente. Los filtros por fecha comparan contra hora local.
//
// 3. 'numero_pedido' es un consecutivo que se reinicia en cada cierre de caja, así
//    que se repite. No se puede usar para localizar un pedido de forma única.
//
// 4. El precio de cada línea queda CONGELADO en detalle_pedidos.precio_unitario.
//    Si después sube el precio del producto, la venta vieja no cambia. Por eso el
//    detalle expone también el precio actual y un indicador de si difiere.
//
// 5. Los pedidos CANCELADOS se excluyen del resumen de ventas por defecto, para
//    no sumar ventas canceladas a los ingresos. El valor real en la base es
//    CANCELADO, que es lo que escribe VentasController. No existe ANULADO.
[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _db;

    public PedidosController(AppDbContext db)
    {
        _db = db;
    }

    // Histórico de pedidos, del más reciente al más antiguo.
    //
    // Filtros opcionales:
    //   ?estado=ENTREGADO           PENDIENTE / ENTREGADO / CANCELADO
    //   ?estadoPago=EFECTIVO        EFECTIVO / QR / TARJETA / PENDIENTE
    //   ?tipo=MESA                  MESA / PARA_LLEVAR / LOCAL
    //   ?idEmpleado=2               pedidos de un cajero concreto
    //   ?idMesa=1                   pedidos de una mesa
    //   ?idPromocion=6              solo ventas con esa promoción
    //   ?idCliente=1                solo ventas facturadas a ese cliente
    //   ?desde=2026-09-20           fecha inicial (inclusive)
    //   ?hasta=2026-09-27           fecha final (inclusive)
    //   ?buscar=juan                busca en nombre del pedido, número y NIT del cliente
    //   ?page=1&pageSize=50         paginación
    [HttpGet]
    [ProducesResponseType(typeof(PedidoPaginadoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PedidoPaginadoDto>> GetAll(
        [FromQuery] string? estado,
        [FromQuery] string? estadoPago,
        [FromQuery] string? tipo,
        [FromQuery] int? idEmpleado,
        [FromQuery] int? idMesa,
        [FromQuery] int? idPromocion,
        [FromQuery] int? idCliente,
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        [FromQuery] string? buscar,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        // Tope para que nadie pida la tabla entera de un jalón.
        pageSize = Math.Clamp(pageSize, 1, 200);
        page = Math.Max(page, 1);

        var consulta = AplicarFiltros(_db.Pedidos.AsNoTracking(), estado, estadoPago, tipo,
            idEmpleado, idMesa, idPromocion, idCliente, desde, hasta, buscar);

        var total = await consulta.CountAsync(ct);

        var pedidos = await consulta
            .OrderByDescending(p => p.FechaCreacion)
            .ThenByDescending(p => p.IdPedido)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PedidoDto
            {
                IdPedido = p.IdPedido,
                TipoPedido = p.TipoPedido,
                Estado = p.Estado,
                EstadoPago = p.EstadoPago,
                FechaCreacion = p.FechaCreacion,
                Subtotal = p.Subtotal,
                Descuento = p.Descuento,
                Total = p.Total,
                NombrePedido = p.NombrePedido,
                NumeroPedido = p.NumeroPedido,
                Observaciones = p.Observaciones,
                IdMesa = p.IdMesa,
                MesaNumero = p.Mesa != null ? p.Mesa.Numero : null,
                IdEmpleado = p.IdEmpleado,
                EmpleadoNombre = p.Empleado != null ? p.Empleado.Nombre : null,
                IdPromocion = p.IdPromocion,
                PromocionNombre = p.Promocion != null ? p.Promocion.Nombre : null,
                IdCliente = p.IdCliente,
                ClienteRazonSocial = p.Cliente != null ? p.Cliente.RazonSocial : null,
                CantidadItems = p.DetallesPedidos!.Count(),
                TotalItems = p.DetallesPedidos!.Sum(d => d.Subtotal)
            })
            .ToListAsync(ct);

        foreach (var pedido in pedidos)
        {
            pedido.SubtotalConsistente = pedido.TotalItems == pedido.Subtotal;
        }

        return Ok(new PedidoPaginadoDto
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = pedidos
        });
    }

    // Totales de ventas. Ver PedidoResumenDto para el detalle de los campos.
    [HttpGet("resumen")]
    [ProducesResponseType(typeof(PedidoResumenDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PedidoResumenDto>> GetResumen(
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        [FromQuery] int? idEmpleado,
        [FromQuery] bool incluirCancelados = false,
        CancellationToken ct = default)
    {
        var consulta = _db.Pedidos.AsNoTracking();

        if (!incluirCancelados)
        {
            // 'CANCELADO' es el valor que escribe VentasController al cancelar un
            // pedido. No existe 'ANULADO' en este sistema: una versión anterior de
            // esta API filtraba por ese nombre y por eso no excluía nada, dejando
            // que las ventas canceladas contaran como ingresos.
            consulta = consulta.Where(p => p.Estado.ToUpper() != "CANCELADO");
        }

        if (idEmpleado.HasValue)
        {
            consulta = consulta.Where(p => p.IdEmpleado == idEmpleado.Value);
        }

        if (desde.HasValue)
        {
            // La columna es timestamp; se compara contra el inicio del día.
            var inicio = desde.Value.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(p => p.FechaCreacion >= inicio);
        }

        if (hasta.HasValue)
        {
            // Se compara contra el FINAL del día para que ?hasta=2026-09-27
            // incluya también los pedidos de las 21:00 de ese día.
            var fin = hasta.Value.ToDateTime(TimeOnly.MaxValue);
            consulta = consulta.Where(p => p.FechaCreacion <= fin);
        }

        var filas = await consulta
            .Select(p => new
            {
                p.EstadoPago,
                p.TipoPedido,
                p.Total,
                Fecha = p.FechaCreacion.Date
            })
            .ToListAsync(ct);

        var resumen = new PedidoResumenDto
        {
            TotalPedidos = filas.Count,
            TotalVentas = filas.Sum(f => f.Total)
        };

        if (filas.Count > 0)
        {
            // Subtotal y descuento necesitan las tres columnas; se recuperan aparte
            // para no traer toda la cabecera solo por dos sumas.
            var importes = await consulta
                .Select(p => new { p.Subtotal, p.Descuento })
                .ToListAsync(ct);

            resumen.TotalSubtotal = importes.Sum(i => i.Subtotal);
            resumen.TotalDescuento = importes.Sum(i => i.Descuento);
            resumen.TicketPromedio = Math.Round(resumen.TotalVentas / filas.Count, 2);
        }

        resumen.PorEstadoPago = filas
            .GroupBy(f => f.EstadoPago)
            .Select(g => new PedidoResumenAgrupado
            {
                Clave = g.Key,
                Pedidos = g.Count(),
                Total = g.Sum(f => f.Total)
            })
            .OrderByDescending(g => g.Total)
            .ToList();

        resumen.PorTipo = filas
            .GroupBy(f => f.TipoPedido)
            .Select(g => new PedidoResumenAgrupado
            {
                Clave = g.Key,
                Pedidos = g.Count(),
                Total = g.Sum(f => f.Total)
            })
            .OrderByDescending(g => g.Total)
            .ToList();

        resumen.PorDia = filas
            .GroupBy(f => f.Fecha)
            .Select(g => new PedidoResumenDia
            {
                Fecha = DateOnly.FromDateTime(g.Key),
                Pedidos = g.Count(),
                Total = g.Sum(f => f.Total)
            })
            .OrderBy(g => g.Fecha)
            .ToList();

        return Ok(resumen);
    }

    // Un pedido con todas sus líneas.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PedidoDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PedidoDetalleDto>> GetById(int id, CancellationToken ct)
    {
        var base_ = await _db.Pedidos
            .AsNoTracking()
            .Where(p => p.IdPedido == id)
            .Select(p => new PedidoDto
            {
                IdPedido = p.IdPedido,
                TipoPedido = p.TipoPedido,
                Estado = p.Estado,
                EstadoPago = p.EstadoPago,
                FechaCreacion = p.FechaCreacion,
                Subtotal = p.Subtotal,
                Descuento = p.Descuento,
                Total = p.Total,
                NombrePedido = p.NombrePedido,
                NumeroPedido = p.NumeroPedido,
                Observaciones = p.Observaciones,
                IdMesa = p.IdMesa,
                MesaNumero = p.Mesa != null ? p.Mesa.Numero : null,
                IdEmpleado = p.IdEmpleado,
                EmpleadoNombre = p.Empleado != null ? p.Empleado.Nombre : null,
                IdPromocion = p.IdPromocion,
                PromocionNombre = p.Promocion != null ? p.Promocion.Nombre : null,
                IdCliente = p.IdCliente,
                ClienteRazonSocial = p.Cliente != null ? p.Cliente.RazonSocial : null,
                CantidadItems = p.DetallesPedidos!.Count(),
                TotalItems = p.DetallesPedidos!.Sum(d => d.Subtotal)
            })
            .FirstOrDefaultAsync(ct);

        if (base_ is null)
        {
            return NotFound();
        }

        base_.SubtotalConsistente = base_.TotalItems == base_.Subtotal;

        var items = await _db.DetallePedidos
            .AsNoTracking()
            .Where(d => d.IdPedido == id)
            .OrderBy(d => d.IdDetalle)
            .Select(d => new PedidoItemDto
            {
                IdDetalle = d.IdDetalle,
                IdProducto = d.IdProducto,
                ProductoNombre = d.Producto != null ? d.Producto.Nombre : null,
                ProductoActivo = d.Producto!.Activo,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal,
                Observacion = d.Observacion,
                ProductoPrecioActual = d.Producto!.Precio
            })
            .ToListAsync(ct);

        foreach (var item in items)
        {
            item.PrecioCambioDesdeVenta =
                item.ProductoPrecioActual.HasValue && item.ProductoPrecioActual != item.PrecioUnitario;
        }

        // El NIT se lee solo para el detalle, que es donde se necesita.
        var nit = await _db.Pedidos
            .AsNoTracking()
            .Where(p => p.IdPedido == id && p.IdCliente != null)
            .Select(p => p.Cliente!.Nit)
            .FirstOrDefaultAsync(ct);

        var detalle = new PedidoDetalleDto
        {
            IdPedido = base_.IdPedido,
            TipoPedido = base_.TipoPedido,
            Estado = base_.Estado,
            EstadoPago = base_.EstadoPago,
            FechaCreacion = base_.FechaCreacion,
            Subtotal = base_.Subtotal,
            Descuento = base_.Descuento,
            Total = base_.Total,
            NombrePedido = base_.NombrePedido,
            NumeroPedido = base_.NumeroPedido,
            Observaciones = base_.Observaciones,
            IdMesa = base_.IdMesa,
            MesaNumero = base_.MesaNumero,
            IdEmpleado = base_.IdEmpleado,
            EmpleadoNombre = base_.EmpleadoNombre,
            IdPromocion = base_.IdPromocion,
            PromocionNombre = base_.PromocionNombre,
            IdCliente = base_.IdCliente,
            ClienteRazonSocial = base_.ClienteRazonSocial,
            ClienteNit = nit,
            CantidadItems = base_.CantidadItems,
            TotalItems = base_.TotalItems,
            SubtotalConsistente = base_.SubtotalConsistente,
            Items = items
        };

        return Ok(detalle);
    }

    // Filtros compartidos por el listado y reutilizables por el resumen.
    private static IQueryable<Pedido> AplicarFiltros(
        IQueryable<Pedido> consulta,
        string? estado, string? estadoPago, string? tipo,
        int? idEmpleado, int? idMesa, int? idPromocion, int? idCliente,
        DateOnly? desde, DateOnly? hasta, string? buscar)
    {
        if (!string.IsNullOrWhiteSpace(estado))
        {
            var v = estado.Trim().ToUpperInvariant();
            consulta = consulta.Where(p => p.Estado.ToUpper() == v);
        }

        if (!string.IsNullOrWhiteSpace(estadoPago))
        {
            var v = estadoPago.Trim().ToUpperInvariant();
            consulta = consulta.Where(p => p.EstadoPago.ToUpper() == v);
        }

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var v = tipo.Trim().ToUpperInvariant();
            consulta = consulta.Where(p => p.TipoPedido.ToUpper() == v);
        }

        if (idEmpleado.HasValue) consulta = consulta.Where(p => p.IdEmpleado == idEmpleado.Value);
        if (idMesa.HasValue) consulta = consulta.Where(p => p.IdMesa == idMesa.Value);
        if (idPromocion.HasValue) consulta = consulta.Where(p => p.IdPromocion == idPromocion.Value);
        if (idCliente.HasValue) consulta = consulta.Where(p => p.IdCliente == idCliente.Value);

        if (desde.HasValue)
        {
            var inicio = desde.Value.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(p => p.FechaCreacion >= inicio);
        }

        if (hasta.HasValue)
        {
            // Fin del día: ?hasta=2026-09-27 incluye los pedidos de esa noche.
            var fin = hasta.Value.ToDateTime(TimeOnly.MaxValue);
            consulta = consulta.Where(p => p.FechaCreacion <= fin);
        }

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(p =>
                (p.NombrePedido != null && EF.Functions.ILike(p.NombrePedido, $"%{texto}%")) ||
                (p.NumeroPedido != null && EF.Functions.ILike(p.NumeroPedido, $"%{texto}%")) ||
                (p.Cliente != null && EF.Functions.ILike(p.Cliente.RazonSocial, $"%{texto}%")) ||
                (p.Cliente != null && EF.Functions.ILike(p.Cliente.Nit, $"%{texto}%")));
        }

        return consulta;
    }
}

// Envoltura de paginación del listado.
public class PedidoPaginadoDto
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<PedidoDto> Items { get; set; } = new();
}
