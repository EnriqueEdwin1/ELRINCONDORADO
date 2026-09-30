using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

[Authorize]
[ApiController]
[Route("api/roles")]
public class RolesWriteController : ControllerBase
{
    private readonly AppDbContext _db;

    public RolesWriteController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Rol rol)
    {
        _db.Roles.Add(rol);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Rol creado exitosamente.", id = rol.IdRol });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] Rol rol)
    {
        if (id != rol.IdRol)
            return BadRequest(new { mensaje = "El ID no coincide." });

        _db.Roles.Update(rol);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Rol actualizado exitosamente." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var rol = await _db.Roles.FindAsync(id);
        if (rol == null)
            return NotFound(new { mensaje = "Rol no encontrado." });

        _db.Roles.Remove(rol);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Rol eliminado exitosamente." });
    }
}
