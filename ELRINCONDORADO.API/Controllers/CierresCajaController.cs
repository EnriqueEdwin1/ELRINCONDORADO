using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

// Módulo Cierres de caja. Solo lectura. Con esto quedan las 20 tablas de
// Supabase expuestas en la API.
//
// DOS ADVERTENCIAS QUE AFECTAN A CÓMO USAR ESTOS DATOS:
//
// 1. 'total_ventas' NO es el total del turno. El MVC lo calcula con
//    ConstruirCierreCaja(), que suma los pedidos de HOY (DateTime.Today) con
//    estado distinto de CANCELADO. Como el turno puede empezar el día anterior,
//    este número deja fuera ventas del turno y puede incluir ventas del turno
//    siguiente. Para el total real del turno está /api/cierres-caja/resumen.
//
// 2. 'cierres_caja.fecha' está en UTC y 'pedidos.fecha_creacion' en hora local.
//    El MVC guarda aquí DateTime.UtcNow, pero en pedidos la hora local. Este
//    controller NO inventa ninguna conversión y compara los instantes tal cual,
//    igual que hace el resumen del turno. Si el servidor que registró los datos
//    estaba en UTC, el turno saldrá inflado por el desfase; se documenta para
//    que quien lo use lo verifique contra el panel del MVC.
//    El detalle NO reproduce 'total_ventas' (día calendario): devuelve el turno
//    real, los pedidos entre el cierre anterior y este.
[ApiController]
[Route("api/cierres-caja")]
public class CierresCajaController : ControllerBase
{
    private readonly AppDbContext _db;

    public CierresCajaController(AppDbContext db)
    {
        _db = db;
    }

    // Histórico de cortes, del más reciente al más antiguo.
    //
    // Filtros opcionales:
    //   ?idEmpleado=2      solo cierres de un empleado
    //   ?desde=2026-09-20  fecha inicial (inclusive)
    //   ?hasta=2026-09-30  fecha final (inclusive)
    //   ?page=1&pageSize=50
    [HttpGet]
    [ProducesResponseType(typeof(CierreCajaPaginadoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CierreCajaPaginadoDto>> GetAll(
        [FromQuery] int? idEmpleado,
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        page = Math.Max(page, 1);

        var consulta = _db.CierresCaja.AsNoTracking();

        if (idEmpleado.HasValue)
        {
            consulta = consulta.Where(c => c.IdEmpleado == idEmpleado.Value);
        }

        if (desde.HasValue)
        {
            // La fecha del cierre es UTC, así que el rango se acota en UTC.
            var inicio = desde.Value.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(c => c.Fecha >= inicio);
        }

        if (hasta.HasValue)
        {
            var fin = hasta.Value.ToDateTime(TimeOnly.MaxValue);
            consulta = consulta.Where(c => c.Fecha <= fin);
        }

        var total = await consulta.CountAsync(ct);

        var cierres = await consulta
            .OrderByDescending(c => c.IdCierre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CierreCajaDto
            {
                IdCierre = c.IdCierre,
                Fecha = c.Fecha,
                IdEmpleado = c.IdEmpleado,
                EmpleadoNombre = c.Empleado != null ? c.Empleado.Nombre : null,
                TotalVentas = c.TotalVentas
            })
            .ToListAsync(ct);

        var ids = cierres.Select(c => c.IdCierre).ToList();
        var empleados = ids.Select(i => cierres.First(c => c.IdCierre == i).IdEmpleado).Distinct().ToList();

        // 'id_empleado' no tiene FK, así que un id puede apuntar a un empleado que
        // ya no existe. Se consulta la lista real en vez de asumir solo el caso 0.
        var empleadosExistentes = await _db.Empleados
            .AsNoTracking()
            .Where(e => empleados.Contains(e.IdEmpleado))
            .Select(e => e.IdEmpleado)
            .ToListAsync(ct);

        // movements.id_cierre no tiene FK, pero sí es un id válido: se cuenta por
        // igualdad y luego se contrasta contra empleados para marcar los huérfanos.
        var conteoMovimientos = await _db.MovimientosInventario
            .AsNoTracking()
            .Where(m => m.IdCierre != null && ids.Contains(m.IdCierre.Value))
            .GroupBy(m => m.IdCierre!.Value)
            .Select(g => new { IdCierre = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.IdCierre, x => x.Cantidad, ct);

        foreach (var cierre in cierres)
        {
            cierre.MovimientosCerrados = conteoMovimientos.GetValueOrDefault(cierre.IdCierre);
            cierre.EmpleadoDesconocido = !empleadosExistentes.Contains(cierre.IdEmpleado);
        }

        return Ok(new CierreCajaPaginadoDto
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = cierres
        });
    }

    // Estado actual de la caja: qué lleva el turno desde el último corte.
    // Equivale al resumen de AdministradorController del MVC.
    [HttpGet("resumen")]
    [ProducesResponseType(typeof(CajaResumenDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CajaResumenDto>> GetResumen(CancellationToken ct)
    {
        var ultimoCierre = await _db.CierresCaja
            .AsNoTracking()
            .OrderByDescending(c => c.IdCierre)
            .FirstOrDefaultAsync(ct);

        // Una venta cuenta como facturada si tiene método de pago asignado y no
        // fue cancelada. Mismo criterio que el MVC.
        var ventas = _db.Pedidos
            .AsNoTracking()
            .Where(p => p.EstadoPago != null && p.EstadoPago != ""
                        && p.EstadoPago != "PENDIENTE"
                        && p.Estado.ToUpper() != "CANCELADO");

        var delTurno = await ventas
            .Where(p => ultimoCierre == null || p.FechaCreacion > UltimoCierreLocal(ultimoCierre))
            .Select(p => new { p.EstadoPago, p.Total, p.FechaCreacion })
            .ToListAsync(ct);

        var ventasHoy = 0;
        var vendidoHoy = 0m;

        // "Hoy" sale del reloj de la base, no del de la API, para que el corte
        // del día no dependa de dónde corra el proceso. SqlQuery<T> con un tipo
        // escalar exige que la columna se llame "Value"; sin ese alias PostgreSQL
        // responde "column s.Value does not exist" (42703).
        var hoy = await _db.Database
            .SqlQuery<DateOnly?>($"SELECT (now() at time zone 'utc')::date AS \"Value\"")
            .FirstOrDefaultAsync(ct);

        if (hoy.HasValue)
        {
            var delDia = await ventas
                .Where(p => p.FechaCreacion >= hoy.Value.ToDateTime(TimeOnly.MinValue)
                            && p.FechaCreacion <= hoy.Value.ToDateTime(TimeOnly.MaxValue))
                .Select(p => p.Total)
                .ToListAsync(ct);

            ventasHoy = delDia.Count;
            vendidoHoy = delDia.Sum();
        }

        var pendientes = await _db.MovimientosInventario
            .AsNoTracking()
            .CountAsync(m => m.IdCierre == null, ct);

        var resumen = new CajaResumenDto
        {
            HayCierres = ultimoCierre != null,
            UltimoCierreId = ultimoCierre?.IdCierre,
            UltimoCierreFecha = ultimoCierre?.Fecha,
            VentasTurno = delTurno.Count,
            VendidoTurno = delTurno.Sum(v => v.Total),
            VentasHoy = ventasHoy,
            VendidoHoy = vendidoHoy,
            MovimientosPendientes = pendientes
        };

        resumen.PorEstadoPago = delTurno
            .GroupBy(v => v.EstadoPago)
            .Select(g => new CajaResumenPago
            {
                EstadoPago = g.Key,
                Pedidos = g.Count(),
                Total = g.Sum(v => v.Total)
            })
            .OrderByDescending(g => g.Total)
            .ToList();

        return Ok(resumen);
    }

    // Un corte con los movimientos de inventario y las ventas que engloba.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CierreCajaDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CierreCajaDetalleDto>> GetById(int id, CancellationToken ct)
    {
        var base_ = await _db.CierresCaja
            .AsNoTracking()
            .Where(c => c.IdCierre == id)
            .Select(c => new CierreCajaDto
            {
                IdCierre = c.IdCierre,
                Fecha = c.Fecha,
                IdEmpleado = c.IdEmpleado,
                EmpleadoNombre = c.Empleado != null ? c.Empleado.Nombre : null,
                TotalVentas = c.TotalVentas
            })
            .FirstOrDefaultAsync(ct);

        if (base_ is null)
        {
            return NotFound();
        }

        var movimientos = await _db.MovimientosInventario
            .AsNoTracking()
            .Where(m => m.IdCierre == id)
            .GroupBy(m => m.TipoMovimiento)
            .Select(g => new CierreCajaMovimientoResumen
            {
                TipoMovimiento = g.Key,
                Cantidad = g.Count(),
                Unidades = g.Sum(m => m.Cantidad)
            })
            .OrderBy(m => m.TipoMovimiento)
            .ToListAsync(ct);

        base_.MovimientosCerrados = movimientos.Sum(m => m.Cantidad);
        base_.EmpleadoDesconocido = base_.EmpleadoNombre is null;

        // El corte cierra el turno que empezó en el cierre anterior, así que las
        // ventas que engloba son las creadas en (cierre anterior, este corte].
        // Sin cierre previo el turno venía abierto y se toma todo lo anterior.
        // Se comparan los instantes crudos, mismo criterio que en /resumen y
        // documentado en la cabecera: si el servidor guardó hora local en
        // pedidos y UTC en cierres, el intervalo sale corrido por ese desfase.
        var cierreAnterior = await _db.CierresCaja
            .AsNoTracking()
            .Where(c => c.IdCierre < id)
            .OrderByDescending(c => c.IdCierre)
            .Select(c => (DateTime?)c.Fecha)
            .FirstOrDefaultAsync(ct);

        var ventasQuery = _db.Pedidos
            .AsNoTracking()
            .Where(p => p.FechaCreacion <= base_.Fecha
                        && p.Estado.ToUpper() != "CANCELADO");

        if (cierreAnterior.HasValue)
        {
            ventasQuery = ventasQuery.Where(p => p.FechaCreacion > cierreAnterior.Value);
        }

        var ventas = await ventasQuery
            .OrderBy(p => p.FechaCreacion)
            .Select(p => new CierreCajaVentaDto
            {
                IdPedido = p.IdPedido,
                NombrePedido = p.NombrePedido,
                NumeroPedido = p.NumeroPedido,
                TipoPedido = p.TipoPedido,
                EstadoPago = p.EstadoPago,
                FechaCreacion = p.FechaCreacion,
                Subtotal = p.Subtotal,
                Descuento = p.Descuento,
                Total = p.Total
            })
            .ToListAsync(ct);

        var detalle = new CierreCajaDetalleDto
        {
            IdCierre = base_.IdCierre,
            Fecha = base_.Fecha,
            IdEmpleado = base_.IdEmpleado,
            EmpleadoNombre = base_.EmpleadoNombre,
            TotalVentas = base_.TotalVentas,
            MovimientosCerrados = base_.MovimientosCerrados,
            EmpleadoDesconocido = base_.EmpleadoDesconocido,
            TurnoDesde = cierreAnterior,
            Movimientos = movimientos,
            VentasIncluidas = ventas
        };

        return Ok(detalle);
    }

    // El cierre se guarda en UTC y las ventas en hora local. Para comparar el
    // turno haría falta el instante del cierre en hora local, que es su valor
    // crudo menos el desfase de la zona del servidor. La sesion de Supabase esta
    // en UTC y los pedidos se guardaron en hora local, asi que el desfase real
    // es el de la maquina que los registro y no se puede leer desde la base.
    //
    // Lo que si se puede hacer sin inventar nada es NO aplicar ninguna conversion:
    // se compara el instante tal cual. Si el servidor guarda hora local, ambas
    // columnas son local y comparar crudo es correcto. Si el servidor estaba en
    // UTC, el desfase es de horas y el turno saldra inflado; se documenta para
    // que quien lo use lo verifique contra el panel del MVC.
    private static DateTime UltimoCierreLocal(CierreCaja cierre) => cierre.Fecha;
}

// Envoltura de paginación del listado de cierres.
public class CierreCajaPaginadoDto
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<CierreCajaDto> Items { get; set; } = new();
}
