using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

// Módulo Insumos. Réplica en solo lectura de Controllers/InsumosController.cs del
// MVC: lista el inventario con su destino y el detalle de un insumo.
[ApiController]
[Route("api/insumos")]
public class InsumosController : ControllerBase
{
    private readonly AppDbContext _db;

    public InsumosController(AppDbContext db)
    {
        _db = db;
    }

    // Equivale a Insumos/Index.
    //
    // Filtros opcionales:
    //   ?activo=true       solo activos
    //   ?destino=3         solo los de un destino (COCINA, MESAS, BEBIDAS...)
    //   ?bajoMinimo=true   solo los que están por debajo del mínimo, replicando la
    //                      regla del resumen de Administrador/Index
    //   ?bajoMinimo=false  solo los que NO están por debajo del mínimo
    //   ?buscar=sal        busca en nombre y descripción
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InsumoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InsumoDto>>> GetAll(
        [FromQuery] bool? activo,
        [FromQuery] int? destino,
        [FromQuery] bool? bajoMinimo,
        [FromQuery] string? buscar,
        CancellationToken ct)
    {
        var consulta = _db.Insumos.AsNoTracking();

        if (activo.HasValue)
        {
            consulta = consulta.Where(i => i.Activo == activo.Value);
        }

        if (destino.HasValue)
        {
            consulta = consulta.Where(i => i.IdDestino == destino.Value);
        }

        if (bajoMinimo.HasValue)
        {
            // Mismo criterio que usa el MVC: se exige un mínimo mayor que cero,
            // si no todo insumo con mínimo 0 aparecería como "bajo mínimo".
            // El filtro se aplica tanto para true como para false, para que el
            // parámetro signifique siempre lo mismo.
            var bajo = bajoMinimo.Value;
            consulta = bajo
                ? consulta.Where(i => i.Activo && i.StockMinimo > 0 && i.StockActual < i.StockMinimo)
                : consulta.Where(i => !i.Activo || i.StockMinimo <= 0 || i.StockActual >= i.StockMinimo);
        }

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(i =>
                EF.Functions.ILike(i.Nombre, $"%{texto}%") ||
                (i.Descripcion != null && EF.Functions.ILike(i.Descripcion, $"%{texto}%")));
        }

        var insumos = await consulta
            .OrderBy(i => i.Nombre)
            .Select(i => new InsumoDto
            {
                IdInsumo = i.IdInsumo,
                Nombre = i.Nombre,
                Descripcion = i.Descripcion,
                UnidadMedida = i.UnidadMedida,
                StockActual = i.StockActual,
                StockMinimo = i.StockMinimo,
                CostoUnitario = i.CostoUnitario,
                Activo = i.Activo,
                IdDestino = i.IdDestino,
                DestinoNombre = i.Destino != null ? i.Destino.Nombre : null,
                BajoMinimo = i.Activo && i.StockMinimo > 0 && i.StockActual < i.StockMinimo
            })
            .ToListAsync(ct);

        return Ok(insumos);
    }

    // Equivale a Insumos/Details/{id}.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InsumoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InsumoDto>> GetById(int id, CancellationToken ct)
    {
        var insumo = await _db.Insumos
            .AsNoTracking()
            .Where(i => i.IdInsumo == id)
            .Select(i => new InsumoDto
            {
                IdInsumo = i.IdInsumo,
                Nombre = i.Nombre,
                Descripcion = i.Descripcion,
                UnidadMedida = i.UnidadMedida,
                StockActual = i.StockActual,
                StockMinimo = i.StockMinimo,
                CostoUnitario = i.CostoUnitario,
                Activo = i.Activo,
                IdDestino = i.IdDestino,
                DestinoNombre = i.Destino != null ? i.Destino.Nombre : null,
                BajoMinimo = i.Activo && i.StockMinimo > 0 && i.StockActual < i.StockMinimo
            })
            .FirstOrDefaultAsync(ct);

        if (insumo is null)
        {
            return NotFound();
        }

        return Ok(insumo);
    }

    // Catálogo de destinos (COCINA, MESAS, BEBIDAS...). Es la tabla auxiliar que
    // usa el filtro ?destino= de este mismo módulo.
    [HttpGet("destinos")]
    [ProducesResponseType(typeof(IEnumerable<DestinoInsumoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DestinoInsumoDto>>> GetDestinos(CancellationToken ct)
    {
        var destinos = await _db.DestinosInsumos
            .AsNoTracking()
            .OrderBy(d => d.Nombre)
            .Select(d => new DestinoInsumoDto
            {
                IdDestino = d.IdDestino,
                Nombre = d.Nombre
            })
            .ToListAsync(ct);

        return Ok(destinos);
    }
}
