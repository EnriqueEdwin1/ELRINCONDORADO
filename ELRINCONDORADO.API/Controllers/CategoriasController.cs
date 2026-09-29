using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

// Módulo Categorías. Réplica en solo lectura de la lógica del
// Controllers/CategoriasController.cs del proyecto MVC, sin vistas ni TempData:
// la API devuelve datos, el MVC sigue pintando las pantallas.
[ApiController]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriasController(AppDbContext db)
    {
        _db = db;
    }

    // Equivale a Categorias/Index: lista todas con su conteo de productos.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetAll(CancellationToken ct)
    {
        var categorias = await _db.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaDto
            {
                IdCategoria = c.IdCategoria,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                // Mismo dato que el MVC trae con Include(c => c.Productos), pero
                // contado en SQL en lugar de traer todas las filas hijas.
                CantidadProductos = _db.Productos.Count(p => p.IdCategoria == c.IdCategoria)
            })
            .ToListAsync(ct);

        return Ok(categorias);
    }

    // Equivale a Categorias/Details/{id}.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaDto>> GetById(int id, CancellationToken ct)
    {
        var categoria = await _db.Categorias
            .AsNoTracking()
            .Where(c => c.IdCategoria == id)
            .Select(c => new CategoriaDto
            {
                IdCategoria = c.IdCategoria,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                CantidadProductos = _db.Productos.Count(p => p.IdCategoria == c.IdCategoria)
            })
            .FirstOrDefaultAsync(ct);

        if (categoria is null)
        {
            return NotFound();
        }

        return Ok(categoria);
    }
}
