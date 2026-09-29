using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

// Módulo Movimientos de inventario. Réplica en solo lectura de
// Controllers/MovimientosInventarioController.cs del MVC.
//
// El MVC excluye a propósito los movimientos de tipo ENTRADA (los que genera una
// compra) porque ya se ven en la pantalla de Compras. Aquí se reproduce esa regla
// y se puede desactivar con ?incluirEntradas=true.
[ApiController]
[Route("api/movimientos-inventario")]
public class MovimientosInventarioController : ControllerBase
{
    private readonly AppDbContext _db;

    public MovimientosInventarioController(AppDbContext db)
    {
        _db = db;
    }

    // Tipos que el MVC muestra en la pantalla de movimientos: salidas por venta,
    // ajustes manuales e ingresos al cancelar un pedido.
    private static readonly string[] TiposVisiblesPorDefecto = { "VENTA", "AJUSTE", "INGRESO" };

    // Filtros opcionales:
    //   ?insumo=2            movimientos de un insumo
    //   ?tipo=VENTA          uno o varios tipos separados por coma
    //   ?empleado=1          movimientos de un empleado
    //   ?idCierre=3          movimientos de un cierre de caja concreto
    //   ?soloTurnoActual=true  solo los que aún no pertenecen a un cierre
    //   ?incluirEntradas=true  suma los ENTRADA, que el MVC oculta a propósito
    //   ?buscar=compra        busca en el motivo
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MovimientoInventarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MovimientoInventarioDto>>> GetAll(
        [FromQuery] int? insumo,
        [FromQuery] string? tipo,
        [FromQuery] int? empleado,
        [FromQuery] int? idCierre,
        [FromQuery] bool? soloTurnoActual,
        [FromQuery] bool? incluirEntradas,
        [FromQuery] string? buscar,
        CancellationToken ct)
    {
        var consulta = _db.MovimientosInventario.AsNoTracking();

        if (insumo.HasValue)
        {
            consulta = consulta.Where(m => m.IdInsumo == insumo.Value);
        }

        if (empleado.HasValue)
        {
            consulta = consulta.Where(m => m.IdEmpleado == empleado.Value);
        }

        if (idCierre.HasValue)
        {
            consulta = consulta.Where(m => m.IdCierre == idCierre.Value);
        }

        if (soloTurnoActual == true)
        {
            consulta = consulta.Where(m => m.IdCierre == null);
        }

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var tipos = tipo
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.ToUpperInvariant())
                .ToList();

            consulta = consulta.Where(m => tipos.Contains(m.TipoMovimiento.ToUpper()));
        }
        else if (incluirEntradas != true)
        {
            // Comportamiento por defecto idéntico al del MVC.
            consulta = consulta.Where(m => TiposVisiblesPorDefecto.Contains(m.TipoMovimiento));
        }

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(m => m.Motivo != null && EF.Functions.ILike(m.Motivo, $"%{texto}%"));
        }

        var movimientos = await consulta
            .OrderByDescending(m => m.Fecha)
            .ThenByDescending(m => m.IdMovimiento)
            .Select(m => new MovimientoInventarioDto
            {
                IdMovimiento = m.IdMovimiento,
                TipoMovimiento = m.TipoMovimiento,
                Cantidad = m.Cantidad,
                Fecha = m.Fecha,
                Motivo = m.Motivo,
                IdInsumo = m.IdInsumo,
                InsumoNombre = m.Insumo != null ? m.Insumo.Nombre : null,
                IdEmpleado = m.IdEmpleado,
                EmpleadoNombre = m.Empleado != null ? m.Empleado.Nombre + " " + m.Empleado.Apellido : null,
                IdCierre = m.IdCierre
            })
            .ToListAsync(ct);

        return Ok(movimientos);
    }

    // Equivale a MovimientosInventario/Details/{id}.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MovimientoInventarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovimientoInventarioDto>> GetById(int id, CancellationToken ct)
    {
        var movimiento = await _db.MovimientosInventario
            .AsNoTracking()
            .Where(m => m.IdMovimiento == id)
            .Select(m => new MovimientoInventarioDto
            {
                IdMovimiento = m.IdMovimiento,
                TipoMovimiento = m.TipoMovimiento,
                Cantidad = m.Cantidad,
                Fecha = m.Fecha,
                Motivo = m.Motivo,
                IdInsumo = m.IdInsumo,
                InsumoNombre = m.Insumo != null ? m.Insumo.Nombre : null,
                IdEmpleado = m.IdEmpleado,
                EmpleadoNombre = m.Empleado != null ? m.Empleado.Nombre + " " + m.Empleado.Apellido : null,
                IdCierre = m.IdCierre
            })
            .FirstOrDefaultAsync(ct);

        if (movimiento is null)
        {
            return NotFound();
        }

        return Ok(movimiento);
    }
}
