using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;
using System.Security.Cryptography;
using System.Text;

namespace ELRINCONDORADO.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
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
                var empleado = await _context.Empleados
                    .Include(e => e.Rol)
                    .FirstOrDefaultAsync(e => e.Usuario == model.Usuario);

                if (empleado == null)
                {
                    _logger.LogWarning($"Intento de login fallido para usuario: {model.Usuario}");
                    return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });
                }

                // Verificar contraseña (en producción usar hashing adecuado)
                if (!VerifyPassword(model.Password, empleado.PasswordHash))
                {
                    _logger.LogWarning($"Contraseña incorrecta para usuario: {model.Usuario}");
                    return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });
                }

                if (empleado.Estado != "ACTIVO")
                {
                    _logger.LogWarning($"Intento de login con usuario inactivo: {model.Usuario}");
                    return Unauthorized(new { mensaje = "Usuario inactivo" });
                }

                // Verificar si el rol tiene acceso al sistema
                var redirectUrl = GetDashboardUrl(empleado.Rol?.Nombre);
                if (string.IsNullOrEmpty(redirectUrl))
                {
                    _logger.LogWarning($"Rol sin acceso al sistema: {empleado.Rol?.Nombre} (usuario: {model.Usuario})");
                    return StatusCode(403, new { mensaje = "Su rol no tiene acceso al sistema" });
                }

                _logger.LogInformation($"Login exitoso para usuario: {model.Usuario}");

                // Guardar información en sesión
                HttpContext.Session.SetString("UsuarioId", empleado.IdEmpleado.ToString());
                HttpContext.Session.SetString("Usuario", empleado.Usuario);
                HttpContext.Session.SetString("Nombre", $"{empleado.Nombre} {empleado.Apellido}");
                HttpContext.Session.SetString("Rol", empleado.Rol?.Nombre ?? "Sin rol");

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
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Método auxiliar para verificar contraseña
        private bool VerifyPassword(string password, string hash)
        {
            // Acepta tanto contraseñas en texto plano como hashes SHA256 en Base64
            if (string.IsNullOrEmpty(hash))
                return false;

            if (string.Equals(password, hash, StringComparison.Ordinal))
                return true;

            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
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

        // Método auxiliar para hashear contraseña
        private string HashPassword(string password)
        {
            // En producción, usar BCrypt
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
