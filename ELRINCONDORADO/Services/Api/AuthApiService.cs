using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace ELRINCONDORADO.Services.Api
{
    public class AuthApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthApiService> _logger;

        public AuthApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> LoginAsync(string usuario, string password)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ElRinconDoradoAPI");
                
                var loginRequest = new
                {
                    Usuario = usuario,
                    Password = password
                };

                var json = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/auth/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Login fallido: {response.StatusCode}");
                    return false;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token))
                {
                    _logger.LogWarning("Login response inválido");
                    return false;
                }

                // Guardar el token JWT en sesión
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Session.SetString("JwtToken", loginResponse.Token);
                    httpContext.Session.SetString("UsuarioId", loginResponse.IdEmpleado.ToString());
                    httpContext.Session.SetString("Usuario", loginResponse.Usuario);
                    httpContext.Session.SetString("Nombre", $"{loginResponse.Nombre} {loginResponse.Apellido}");
                    httpContext.Session.SetString("Rol", loginResponse.Rol);

                    // Crear claims para autenticación de cookies
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, loginResponse.IdEmpleado.ToString()),
                        new Claim(ClaimTypes.Name, loginResponse.Usuario),
                        new Claim(ClaimTypes.Role, loginResponse.Rol),
                        new Claim("nombre_completo", $"{loginResponse.Nombre} {loginResponse.Apellido}")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(loginResponse.ExpiraEnHoras)
                    };

                    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                        new ClaimsPrincipal(claimsIdentity), authProperties);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                return false;
            }
        }

        public string? GetToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.Session.GetString("JwtToken");
        }

        public async Task LogoutAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                httpContext.Session.Clear();
            }
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string TipoToken { get; set; } = string.Empty;
        public int ExpiraEnHoras { get; set; }
        public int IdEmpleado { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public string Rol { get; set; } = string.Empty;
    }
}
