using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

// Módulo Roles. Réplica en solo lectura de Controllers/RolesController.cs del MVC.
[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RolesController(AppDbContext db)
    {
        _db = db;
    }

    // Equivale a Roles/Index.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RolDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RolDto>>> GetAll(CancellationToken ct)
    {
        var roles = await _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Nombre)
            .Select(r => new RolDto
            {
                IdRol = r.IdRol,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                CantidadEmpleados = _db.Empleados.Count(e => e.IdRol == r.IdRol)
            })
            .ToListAsync(ct);

        return Ok(roles);
    }

    // Equivale a Roles/Details/{id}.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RolDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RolDto>> GetById(int id, CancellationToken ct)
    {
        var rol = await _db.Roles
            .AsNoTracking()
            .Where(r => r.IdRol == id)
            .Select(r => new RolDto
            {
                IdRol = r.IdRol,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                CantidadEmpleados = _db.Empleados.Count(e => e.IdRol == r.IdRol)
            })
            .FirstOrDefaultAsync(ct);

        if (rol is null)
        {
            return NotFound();
        }

        return Ok(rol);
    }
}
