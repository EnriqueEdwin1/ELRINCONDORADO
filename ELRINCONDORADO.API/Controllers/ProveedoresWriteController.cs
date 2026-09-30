using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

[Authorize]
[ApiController]
[Route("api/proveedores")]
public class ProveedoresWriteController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProveedoresWriteController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Proveedor proveedor)
    {
        _db.Proveedores.Add(proveedor);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Proveedor creado exitosamente.", id = proveedor.IdProveedor });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] Proveedor proveedor)
    {
        if (id != proveedor.IdProveedor)
            return BadRequest(new { mensaje = "El ID no coincide." });

        _db.Proveedores.Update(proveedor);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Proveedor actualizado exitosamente." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var proveedor = await _db.Proveedores.FindAsync(id);
        if (proveedor == null)
            return NotFound(new { mensaje = "Proveedor no encontrado." });

        _db.Proveedores.Remove(proveedor);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Proveedor eliminado exitosamente." });
    }
}
