using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

[Authorize]
[ApiController]
[Route("api/insumos")]
public class InsumosWriteController : ControllerBase
{
    private readonly AppDbContext _db;

    public InsumosWriteController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Insumo insumo)
    {
        _db.Insumos.Add(insumo);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Insumo creado exitosamente.", id = insumo.IdInsumo });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] Insumo insumo)
    {
        if (id != insumo.IdInsumo)
            return BadRequest(new { mensaje = "El ID no coincide." });

        _db.Insumos.Update(insumo);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Insumo actualizado exitosamente." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var insumo = await _db.Insumos.FindAsync(id);
        if (insumo == null)
            return NotFound(new { mensaje = "Insumo no encontrado." });

        _db.Insumos.Remove(insumo);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Insumo eliminado exitosamente." });
    }
}
