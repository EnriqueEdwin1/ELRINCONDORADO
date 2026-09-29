using Microsoft.AspNetCore.Mvc;
using ELRINCONDORADO.Models;
using ELRINCONDORADO.Services.Api;

namespace ELRINCONDORADO.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthApiService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthApiService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // GET: Auth/Login
        public IActionResult Login()
        {
            return View("~/Views/Home/Index.cshtml");
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { mensaje = "Datos inválidos" });
            }

            try
            {
                var loginExitoso = await _authService.LoginAsync(model.Usuario, model.Password);

                if (!loginExitoso)
                {
                    _logger.LogWarning($"Intento de login fallido para usuario: {model.Usuario}");
                    return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });
                }

                var rol = HttpContext.Session.GetString("Rol");
                var redirectUrl = GetDashboardUrl(rol);

                if (string.IsNullOrEmpty(redirectUrl))
                {
                    _logger.LogWarning($"Rol sin acceso al sistema: {rol} (usuario: {model.Usuario})");
                    return StatusCode(403, new { mensaje = "Su rol no tiene acceso al sistema" });
                }

                _logger.LogInformation($"Login exitoso para usuario: {model.Usuario}");

                return Ok(new { 
                    mensaje = "Login exitoso",
                    redirectUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                return StatusCode(500, new { mensaje = "Error en el servidor" });
            }
        }

        // POST: Auth/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Login");
        }

        // Método auxiliar para devolver la URL del panel según el rol
        private string? GetDashboardUrl(string? rol)
        {
            return rol?.ToUpper() switch
            {
                "ADMINISTRADOR" => "/Administrador",
                "CAJERO" => "/Cajero",
                "MESERO" => "/Mesero",
                "COCINERO" => "/Cocina",
                "ALMACEN" => "/Almacen",
                _ => null
            };
        }
    }
}
