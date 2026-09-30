using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

[Authorize]
[ApiController]
[Route("api/categorias")]
public class CategoriasWriteController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriasWriteController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Categoria categoria)
    {
        if (await _db.Categorias.AnyAsync(c => c.Nombre == categoria.Nombre))
        {
            return BadRequest(new { mensaje = "Ese nombre de categoría ya existe." });
        }

        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Categoría creada exitosamente." });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] Categoria categoria)
    {
        if (id != categoria.IdCategoria)
        {
            return BadRequest(new { mensaje = "El ID no coincide." });
        }

        if (await _db.Categorias.AnyAsync(c => c.Nombre == categoria.Nombre && c.IdCategoria != categoria.IdCategoria))
        {
            return BadRequest(new { mensaje = "Ese nombre de categoría ya existe." });
        }

        _db.Categorias.Update(categoria);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Categoría actualizada exitosamente." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _db.Categorias.FindAsync(id);
        if (categoria == null)
        {
            return NotFound(new { mensaje = "Categoría no encontrada." });
        }

        if (await _db.Productos.AnyAsync(p => p.IdCategoria == id))
        {
            return BadRequest(new { mensaje = "No se puede eliminar: la categoría tiene productos asociados." });
        }

        _db.Categorias.Remove(categoria);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Categoría eliminada exitosamente." });
    }
}
