using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

[Authorize]
[ApiController]
[Route("api/recetas")]
public class RecetasWriteController : ControllerBase
{
    private readonly AppDbContext _db;

    public RecetasWriteController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Receta receta)
    {
        _db.Recetas.Add(receta);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Receta creada exitosamente.", id = receta.IdReceta });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] Receta receta)
    {
        if (id != receta.IdReceta)
            return BadRequest(new { mensaje = "El ID no coincide." });

        _db.Recetas.Update(receta);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Receta actualizada exitosamente." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var receta = await _db.Recetas.FindAsync(id);
        if (receta == null)
            return NotFound(new { mensaje = "Receta no encontrada." });

        _db.Recetas.Remove(receta);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Receta eliminada exitosamente." });
    }
}
