using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

[Authorize]
[ApiController]
[Route("api/destinos-insumos")]
public class DestinosInsumosWriteController : ControllerBase
{
    private readonly AppDbContext _db;

    public DestinosInsumosWriteController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DestinoInsumo destino)
    {
        _db.DestinosInsumos.Add(destino);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Destino creado exitosamente.", id = destino.IdDestino });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] DestinoInsumo destino)
    {
        if (id != destino.IdDestino)
            return BadRequest(new { mensaje = "El ID no coincide." });

        _db.DestinosInsumos.Update(destino);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Destino actualizado exitosamente." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var destino = await _db.DestinosInsumos.FindAsync(id);
        if (destino == null)
            return NotFound(new { mensaje = "Destino no encontrado." });

        _db.DestinosInsumos.Remove(destino);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Destino eliminado exitosamente." });
    }
}
