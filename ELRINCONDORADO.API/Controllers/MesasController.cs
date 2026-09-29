using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

// Módulo Mesas. Réplica en solo lectura de Controllers/MesasController.cs del MVC.
[ApiController]
[Route("api/mesas")]
public class MesasController : ControllerBase
{
    private readonly AppDbContext _db;

    public MesasController(AppDbContext db)
    {
        _db = db;
    }

    // Equivale a Mesas/Index. El filtro ?estado= reproduce la regla que aplica el
    // POS del Cajero, que solo ofrece mesas con Estado == "DISPONIBLE".
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MesaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MesaDto>>> GetAll(
        [FromQuery] string? estado,
        CancellationToken ct)
    {
        var consulta = _db.Mesas.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoBuscado = estado.Trim().ToUpperInvariant();
            consulta = consulta.Where(m => m.Estado == estadoBuscado);
        }

        var mesas = await consulta
            .OrderBy(m => m.Numero)
            .Select(m => new MesaDto
            {
                IdMesa = m.IdMesa,
                Numero = m.Numero,
                Capacidad = m.Capacidad,
                Estado = m.Estado
            })
            .ToListAsync(ct);

        return Ok(mesas);
    }

    // Equivale a Mesas/Details/{id}.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MesaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MesaDto>> GetById(int id, CancellationToken ct)
    {
        var mesa = await _db.Mesas
            .AsNoTracking()
            .Where(m => m.IdMesa == id)
            .Select(m => new MesaDto
            {
                IdMesa = m.IdMesa,
                Numero = m.Numero,
                Capacidad = m.Capacidad,
                Estado = m.Estado
            })
            .FirstOrDefaultAsync(ct);

        if (mesa is null)
        {
            return NotFound();
        }

        return Ok(mesa);
    }
}
