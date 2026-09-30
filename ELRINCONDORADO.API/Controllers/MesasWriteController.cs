using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

[Authorize]
[ApiController]
[Route("api/mesas")]
public class MesasWriteController : ControllerBase
{
    private readonly AppDbContext _db;

    public MesasWriteController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Mesa mesa)
    {
        _db.Mesas.Add(mesa);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Mesa creada exitosamente.", id = mesa.IdMesa });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] Mesa mesa)
    {
        if (id != mesa.IdMesa)
            return BadRequest(new { mensaje = "El ID no coincide." });

        _db.Mesas.Update(mesa);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Mesa actualizada exitosamente." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var mesa = await _db.Mesas.FindAsync(id);
        if (mesa == null)
            return NotFound(new { mensaje = "Mesa no encontrada." });

        _db.Mesas.Remove(mesa);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Mesa eliminada exitosamente." });
    }
}
