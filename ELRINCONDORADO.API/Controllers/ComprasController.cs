using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

// Módulo Compras. Solo lectura.
//
// LO MÁS IMPORTANTE DE ESTE MÓDULO: no existe columna 'id_compra' en
// 'movimientos_inventario'. Cuando se registra una compra, el POS inserta los
// insumos como movimientos de tipo ENTRADA y los vincula a la compra SOLO
// escribiendo el id en texto dentro de 'motivo', con el formato "COMPRA #<id>".
//
// Eso significa que:
//
//   * No se puede hacer un JOIN entre compras y movimientos_inventario.
//   * El vínculo depende de que nadie edite ese texto.
//   * Si el formato cambia, la trazabilidad se rompe en silencio: el insumo entró
//     al stock pero ya no se sabe de qué compra vino.
//
// Por eso el detalle de cada línea trae 'entradaRegistrada', y la compra trae
// 'lineasConEntradaRegistrada' e 'inventarioCompleto'. No se asume que el
// vínculo funciona: se verifica y se informa.
//
// Ojo también a que 'detalle_compras.cantidad' es numeric(18,8) y admite
// fracciones (kilos, litros), a diferencia de las cantidades de pedidos y
// promociones, que son enteras.
[ApiController]
[Route("api/compras")]
public class ComprasController : ControllerBase
{
    private readonly AppDbContext _db;

    public ComprasController(AppDbContext db)
    {
        _db = db;
    }

    // Histórico de compras, de la más reciente a la más antigua.
    //
    // Filtros opcionales:
    //   ?estado=REGISTRADA
    //   ?idProveedor=2       compras de un proveedor
    //   ?idEmpleado=1        compras registradas por un empleado
    //   ?desde=2026-09-01    fecha inicial (inclusive)
    //   ?hasta=2026-09-30    fecha final (inclusive, hasta el fin del día)
    //   ?buscar=juan         busca en nombre del proveedor y nombre del empleado
    //   ?page=1&pageSize=50
    [HttpGet]
    [ProducesResponseType(typeof(CompraPaginadoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CompraPaginadoDto>> GetAll(
        [FromQuery] string? estado,
        [FromQuery] int? idProveedor,
        [FromQuery] int? idEmpleado,
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        [FromQuery] string? buscar,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        page = Math.Max(page, 1);

        var consulta = AplicarFiltros(_db.Compras.AsNoTracking(), estado, idProveedor,
            idEmpleado, desde, hasta, buscar);

        var total = await consulta.CountAsync(ct);

        var compras = await consulta
            .OrderByDescending(c => c.FechaCompra)
            .ThenByDescending(c => c.IdCompra)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CompraDto
            {
                IdCompra = c.IdCompra,
                IdProveedor = c.IdProveedor,
                ProveedorNombre = c.Proveedor != null ? c.Proveedor.Nombre : null,
                IdEmpleado = c.IdEmpleado,
                EmpleadoNombre = c.Empleado != null ? c.Empleado.Nombre : null,
                FechaCompra = c.FechaCompra,
                Subtotal = c.Subtotal,
                Descuento = c.Descuento,
                Total = c.Total,
                Estado = c.Estado,
                Lineas = c.DetalleCompras!.Count(),
                SumaDetalle = c.DetalleCompras!.Sum(d => d.Subtotal)
            })
            .ToListAsync(ct);

        foreach (var compra in compras)
        {
            compra.SubtotalConsistente = compra.SumaDetalle == compra.Subtotal;
        }

        return Ok(new CompraPaginadoDto
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = compras
        });
    }

    // Totales de compras a proveedores.
    [HttpGet("resumen")]
    [ProducesResponseType(typeof(CompraResumenDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CompraResumenDto>> GetResumen(
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        [FromQuery] int? idProveedor,
        CancellationToken ct)
    {
        var consulta = _db.Compras.AsNoTracking();

        // Excluir compras anuladas del resumen
        consulta = consulta.Where(c => c.Estado != "ANULADO");

        if (idProveedor.HasValue)
        {
            consulta = consulta.Where(c => c.IdProveedor == idProveedor.Value);
        }

        if (desde.HasValue)
        {
            var inicio = desde.Value.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(c => c.FechaCompra >= inicio);
        }

        if (hasta.HasValue)
        {
            var fin = hasta.Value.ToDateTime(TimeOnly.MaxValue);
            consulta = consulta.Where(c => c.FechaCompra <= fin);
        }

        var filas = await consulta
            .Select(c => new
            {
                c.IdProveedor,
                Nombre = c.Proveedor != null ? c.Proveedor.Nombre : null,
                c.Subtotal,
                c.Descuento,
                c.Total,
                Fecha = c.FechaCompra.Date
            })
            .ToListAsync(ct);

        var resumen = new CompraResumenDto
        {
            TotalCompras = filas.Count,
            TotalSubtotal = filas.Sum(f => f.Subtotal),
            TotalDescuento = filas.Sum(f => f.Descuento),
            TotalComprado = filas.Sum(f => f.Total)
        };

        if (filas.Count > 0)
        {
            resumen.CompraPromedio = Math.Round(resumen.TotalComprado / filas.Count, 2);
        }

        resumen.PorProveedor = filas
            .GroupBy(f => new { f.IdProveedor, f.Nombre })
            .Select(g => new CompraResumenAgrupado
            {
                IdProveedor = g.Key.IdProveedor,
                ProveedorNombre = g.Key.Nombre,
                Compras = g.Count(),
                Total = g.Sum(f => f.Total)
            })
            .OrderByDescending(g => g.Total)
            .ToList();

        resumen.PorDia = filas
            .GroupBy(f => f.Fecha)
            .Select(g => new CompraResumenDia
            {
                Fecha = DateOnly.FromDateTime(g.Key),
                Compras = g.Count(),
                Total = g.Sum(f => f.Total)
            })
            .OrderBy(g => g.Fecha)
            .ToList();

        return Ok(resumen);
    }

    // Una compra con sus insumos y el estado de su entrada a inventario.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CompraDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompraDetalleDto>> GetById(int id, CancellationToken ct)
    {
        var base_ = await _db.Compras
            .AsNoTracking()
            .Where(c => c.IdCompra == id)
            .Select(c => new CompraDto
            {
                IdCompra = c.IdCompra,
                IdProveedor = c.IdProveedor,
                ProveedorNombre = c.Proveedor != null ? c.Proveedor.Nombre : null,
                IdEmpleado = c.IdEmpleado,
                EmpleadoNombre = c.Empleado != null ? c.Empleado.Nombre : null,
                FechaCompra = c.FechaCompra,
                Subtotal = c.Subtotal,
                Descuento = c.Descuento,
                Total = c.Total,
                Estado = c.Estado,
                Lineas = c.DetalleCompras!.Count(),
                SumaDetalle = c.DetalleCompras!.Sum(d => d.Subtotal)
            })
            .FirstOrDefaultAsync(ct);

        if (base_ is null)
        {
            return NotFound();
        }

        base_.SubtotalConsistente = base_.SumaDetalle == base_.Subtotal;

        var items = await _db.DetalleCompras
            .AsNoTracking()
            .Where(d => d.IdCompra == id)
            .OrderBy(d => d.IdDetalleCompra)
            .Select(d => new CompraItemDto
            {
                IdDetalleCompra = d.IdDetalleCompra,
                IdInsumo = d.IdInsumo,
                InsumoNombre = d.Insumo != null ? d.Insumo.Nombre : null,
                InsumoActivo = d.Insumo!.Activo,
                Cantidad = d.Cantidad,
                CostoUnitario = d.CostoUnitario,
                Subtotal = d.Subtotal,
                CostoUnitarioActual = d.Insumo!.CostoUnitario
            })
            .ToListAsync(ct);

        // Se reconstruye el vínculo por texto, que es lo único que existe.
        // 'COMPRA #<id_compra>' es el formato que escribe el POS.
        var motivo = $"COMPRA #{id}";

        var entradas = await _db.MovimientosInventario
            .AsNoTracking()
            .Where(m => m.TipoMovimiento == "ENTRADA" && m.Motivo == motivo)
            .Select(m => new { m.IdInsumo, m.Cantidad })
            .ToListAsync(ct);

        foreach (var item in items)
        {
            item.CostoCambioDesdeCompra = item.CostoUnitarioActual.HasValue
                && item.CostoUnitarioActual != item.CostoUnitario;

            // La entrada tiene que existir Y con la misma cantidad, porque un
            // insumo puede aparecer dos veces en la misma compra.
            item.EntradaRegistrada = entradas.Any(e => e.IdInsumo == item.IdInsumo && e.Cantidad == item.Cantidad);
        }

        var conEntrada = items.Count(i => i.EntradaRegistrada);

        var detalle = new CompraDetalleDto
        {
            IdCompra = base_.IdCompra,
            IdProveedor = base_.IdProveedor,
            ProveedorNombre = base_.ProveedorNombre,
            IdEmpleado = base_.IdEmpleado,
            EmpleadoNombre = base_.EmpleadoNombre,
            FechaCompra = base_.FechaCompra,
            Subtotal = base_.Subtotal,
            Descuento = base_.Descuento,
            Total = base_.Total,
            Estado = base_.Estado,
            Lineas = base_.Lineas,
            SumaDetalle = base_.SumaDetalle,
            SubtotalConsistente = base_.SubtotalConsistente,
            Items = items,
            LineasConEntradaRegistrada = conEntrada,
            LineasTotales = items.Count,
            InventarioCompleto = conEntrada == items.Count
        };

        return Ok(detalle);
    }

    private static IQueryable<Compra> AplicarFiltros(
        IQueryable<Compra> consulta,
        string? estado, int? idProveedor, int? idEmpleado,
        DateOnly? desde, DateOnly? hasta, string? buscar)
    {
        if (!string.IsNullOrWhiteSpace(estado))
        {
            var v = estado.Trim().ToUpperInvariant();
            consulta = consulta.Where(c => c.Estado.ToUpper() == v);
        }

        if (idProveedor.HasValue) consulta = consulta.Where(c => c.IdProveedor == idProveedor.Value);
        if (idEmpleado.HasValue) consulta = consulta.Where(c => c.IdEmpleado == idEmpleado.Value);

        if (desde.HasValue)
        {
            var inicio = desde.Value.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(c => c.FechaCompra >= inicio);
        }

        if (hasta.HasValue)
        {
            // Fin del día, para que ?hasta=X incluya las compras de esa noche.
            var fin = hasta.Value.ToDateTime(TimeOnly.MaxValue);
            consulta = consulta.Where(c => c.FechaCompra <= fin);
        }

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(c =>
                (c.Proveedor != null && EF.Functions.ILike(c.Proveedor.Nombre, $"%{texto}%")) ||
                (c.Empleado != null && EF.Functions.ILike(c.Empleado.Nombre, $"%{texto}%")));
        }

        return consulta;
    }
}

// Envoltura de paginación del listado.
public class CompraPaginadoDto
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<CompraDto> Items { get; set; } = new();
}
