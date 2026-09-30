using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

[Authorize]
[ApiController]
[Route("api/cierres-caja")]
public class CierresCajaWriteController : ControllerBase
{
    private readonly AppDbContext _db;

    public CierresCajaWriteController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CierreCaja cierre)
    {
        _db.CierresCaja.Add(cierre);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Cierre de caja creado exitosamente.", id = cierre.IdCierre });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] CierreCaja cierre)
    {
        if (id != cierre.IdCierre)
            return BadRequest(new { mensaje = "El ID no coincide." });

        _db.CierresCaja.Update(cierre);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Cierre de caja actualizado exitosamente." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cierre = await _db.CierresCaja.FindAsync(id);
        if (cierre == null)
            return NotFound(new { mensaje = "Cierre de caja no encontrado." });

        _db.CierresCaja.Remove(cierre);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Cierre de caja eliminado exitosamente." });
    }
}
