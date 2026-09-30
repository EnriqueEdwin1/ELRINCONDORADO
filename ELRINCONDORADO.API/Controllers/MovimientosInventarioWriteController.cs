using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

[Authorize]
[ApiController]
[Route("api/movimientos-inventario")]
public class MovimientosInventarioWriteController : ControllerBase
{
    private readonly AppDbContext _db;

    public MovimientosInventarioWriteController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MovimientoInventario movimiento)
    {
        _db.MovimientosInventario.Add(movimiento);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Movimiento creado exitosamente.", id = movimiento.IdMovimiento });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] MovimientoInventario movimiento)
    {
        if (id != movimiento.IdMovimiento)
            return BadRequest(new { mensaje = "El ID no coincide." });

        _db.MovimientosInventario.Update(movimiento);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Movimiento actualizado exitosamente." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var movimiento = await _db.MovimientosInventario.FindAsync(id);
        if (movimiento == null)
            return NotFound(new { mensaje = "Movimiento no encontrado." });

        _db.MovimientosInventario.Remove(movimiento);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Movimiento eliminado exitosamente." });
    }
}
