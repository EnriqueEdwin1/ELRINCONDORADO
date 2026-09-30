using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

[Authorize]
[ApiController]
[Route("api/empleados")]
public class EmpleadosWriteController : ControllerBase
{
    private readonly AppDbContext _db;

    public EmpleadosWriteController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Empleado empleado)
    {
        if (await _db.Empleados.AnyAsync(e => e.Usuario == empleado.Usuario))
        {
            return BadRequest(new { mensaje = "Ese nombre de usuario ya existe." });
        }

        _db.Empleados.Add(empleado);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Empleado creado exitosamente.", id = empleado.IdEmpleado });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] Empleado empleado)
    {
        if (id != empleado.IdEmpleado)
            return BadRequest(new { mensaje = "El ID no coincide." });

        if (await _db.Empleados.AnyAsync(e => e.Usuario == empleado.Usuario && e.IdEmpleado != empleado.IdEmpleado))
        {
            return BadRequest(new { mensaje = "Ese nombre de usuario ya existe." });
        }

        _db.Empleados.Update(empleado);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Empleado actualizado exitosamente." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var empleado = await _db.Empleados.FindAsync(id);
        if (empleado == null)
            return NotFound(new { mensaje = "Empleado no encontrado." });

        _db.Empleados.Remove(empleado);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Empleado eliminado exitosamente." });
    }
}
