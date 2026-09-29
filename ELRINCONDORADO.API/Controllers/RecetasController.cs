using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

// Módulo Recetas. En solo lectura: cada receta con los insumos que consume.
// También se puede consultar la receta de un producto, aunque también viene
// incluida en GET /api/productos/{id}.
[ApiController]
[Route("api/recetas")]
public class RecetasController : ControllerBase
{
    private readonly AppDbContext _db;

    public RecetasController(AppDbContext db)
    {
        _db = db;
    }

    // Filtros opcionales:
    //   ?producto=5   solo la receta de ese producto
    //   ?activo=true  solo recetas activas
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RecetaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RecetaDto>>> GetAll(
        [FromQuery] int? producto,
        [FromQuery] bool? activo,
        CancellationToken ct)
    {
        var consulta = _db.Recetas.AsNoTracking();

        if (producto.HasValue)
        {
            consulta = consulta.Where(r => r.IdProducto == producto.Value);
        }

        if (activo.HasValue)
        {
            consulta = consulta.Where(r => r.Activo == activo.Value);
        }

        var recetas = await consulta
            .OrderByDescending(r => r.IdReceta)
            .Select(r => new RecetaDto
            {
                IdReceta = r.IdReceta,
                IdProducto = r.IdProducto,
                ProductoNombre = r.Producto != null ? r.Producto.Nombre : null,
                Descripcion = r.Descripcion,
                Activo = r.Activo,
                Detalles = r.DetalleRecetas!
                    .OrderBy(d => d.Insumo!.Nombre)
                    .Select(d => new RecetaDetalleDto
                    {
                        IdDetalleReceta = d.IdDetalleReceta,
                        IdInsumo = d.IdInsumo,
                        InsumoNombre = d.Insumo != null ? d.Insumo.Nombre : null,
                        Cantidad = d.Cantidad,
                        UnidadMedida = d.UnidadMedida
                    })
                    .ToList()
            })
            .ToListAsync(ct);

        return Ok(recetas);
    }

    // Equivale a DetalleRecetas de una receta concreta.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RecetaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecetaDto>> GetById(int id, CancellationToken ct)
    {
        var receta = await _db.Recetas
            .AsNoTracking()
            .Where(r => r.IdReceta == id)
            .Select(r => new RecetaDto
            {
                IdReceta = r.IdReceta,
                IdProducto = r.IdProducto,
                ProductoNombre = r.Producto != null ? r.Producto.Nombre : null,
                Descripcion = r.Descripcion,
                Activo = r.Activo,
                Detalles = r.DetalleRecetas!
                    .OrderBy(d => d.Insumo!.Nombre)
                    .Select(d => new RecetaDetalleDto
                    {
                        IdDetalleReceta = d.IdDetalleReceta,
                        IdInsumo = d.IdInsumo,
                        InsumoNombre = d.Insumo != null ? d.Insumo.Nombre : null,
                        Cantidad = d.Cantidad,
                        UnidadMedida = d.UnidadMedida
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (receta is null)
        {
            return NotFound();
        }

        return Ok(receta);
    }
}
