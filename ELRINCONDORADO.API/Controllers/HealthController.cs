using ELRINCONDORADO.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

// Controlador de humo temporal: comprueba que la API arranca y que la cadena
// API -> EF Core -> Npgsql -> Supabase PostgreSQL responde. No contiene logica
// de negocio.
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() =>
        Ok(new { proyecto = "ELRINCONDORADO.API", estado = "activo" });

    // Prueba de SOLO LECTURA contra una tabla que ya existe en Supabase.
    // AsNoTracking() evita el seguimiento de entidades, de modo que EF Core
    // nunca puede enviar un INSERT/UPDATE/DELETE por esta ruta.
    [HttpGet("database")]
    public async Task<IActionResult> Database(
        [FromServices] AppDbContext db,
        CancellationToken ct)
    {
        var hayConexion = await db.Database.CanConnectAsync(ct);

        if (!hayConexion)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                conexion = false,
                mensaje = "No se pudo conectar con Supabase PostgreSQL."
            });
        }

        var muestra = await db.Configuracion
            .AsNoTracking()
            .OrderBy(c => c.Clave)
            .Take(5)
            .Select(c => new { c.Clave, c.Valor })
            .ToListAsync(ct);

        var totalEnTabla = await db.Configuracion.AsNoTracking().CountAsync(ct);

        return Ok(new
        {
            conexion = true,
            cadena = "ELRINCONDORADO.API -> EF Core -> Npgsql -> Supabase PostgreSQL",
            tablaProbada = "configuracion",
            modo = "solo lectura (AsNoTracking)",
            filasEnTabla = totalEnTabla,
            filasLeidas = muestra.Count,
            muestra
        });
    }
}
