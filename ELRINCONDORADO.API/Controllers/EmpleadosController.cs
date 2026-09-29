using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

// Módulo Empleados. Réplica en solo lectura de Controllers/EmpleadosController.cs
// del MVC. Las reglas de escritura (usuario único, no crear ADMINISTRADOR, etc.)
// siguen únicamente en el MVC hasta que la API tenga autenticación.
[ApiController]
[Route("api/empleados")]
public class EmpleadosController : ControllerBase
{
    private readonly AppDbContext _db;

    public EmpleadosController(AppDbContext db)
    {
        _db = db;
    }

    // Equivale a Empleados/Index. Acepta filtros opcionales por rol y estado.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmpleadoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmpleadoDto>>> GetAll(
        [FromQuery] int? rol,
        [FromQuery] string? estado,
        CancellationToken ct)
    {
        var consulta = _db.Empleados.AsNoTracking();

        if (rol is not null)
        {
            consulta = consulta.Where(e => e.IdRol == rol.Value);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoBuscado = estado.Trim().ToUpperInvariant();
            consulta = consulta.Where(e => e.Estado == estadoBuscado);
        }

        var empleados = await consulta
            .OrderBy(e => e.Nombre)
            .Select(e => new EmpleadoDto
            {
                IdEmpleado = e.IdEmpleado,
                Nombre = e.Nombre,
                Apellido = e.Apellido,
                Usuario = e.Usuario,
                Telefono = e.Telefono,
                FechaContratacion = e.FechaContratacion,
                Estado = e.Estado,
                IdRol = e.IdRol,
                // Mismo dato que el MVC trae con Include(e => e.Rol)
                RolNombre = e.Rol!.Nombre
            })
            .ToListAsync(ct);

        return Ok(empleados);
    }

    // Equivale a Empleados/Details/{id}.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmpleadoDto>> GetById(int id, CancellationToken ct)
    {
        var empleado = await _db.Empleados
            .AsNoTracking()
            .Where(e => e.IdEmpleado == id)
            .Select(e => new EmpleadoDto
            {
                IdEmpleado = e.IdEmpleado,
                Nombre = e.Nombre,
                Apellido = e.Apellido,
                Usuario = e.Usuario,
                Telefono = e.Telefono,
                FechaContratacion = e.FechaContratacion,
                Estado = e.Estado,
                IdRol = e.IdRol,
                RolNombre = e.Rol!.Nombre
            })
            .FirstOrDefaultAsync(ct);

        if (empleado is null)
        {
            return NotFound();
        }

        return Ok(empleado);
    }
}
