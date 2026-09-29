using ELRINCONDORADO.API.Models.DTOs;
using ELRINCONDORADO.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELRINCONDORADO.API.Controllers;

// Autenticacion de la API. Valida contra la tabla 'empleados' y devuelve un JWT.
// Este es el unico punto de la API que puede leer 'password_hash'.
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AutenticacionService _auth;

    public AuthController(AutenticacionService auth)
    {
        _auth = auth;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var (respuesta, error) = await _auth.LoginAsync(request.Usuario, request.Password, ct);

        if (error is not null)
        {
            return Unauthorized(new { error });
        }

        return Ok(respuesta);
    }
}
