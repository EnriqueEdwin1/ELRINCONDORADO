using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ELRINCONDORADO.API.Data.Auth;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ELRINCONDORADO.API.Services;

// Emite el JWT tras validar las credenciales contra la tabla 'empleados'.
public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public LoginResponse Emitir(CredencialesEmpleado credencial)
    {
        var horas = int.TryParse(_config["Jwt:ExpirationHours"], out var h) && h > 0 ? h : 8;
        var claveFirma = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Clave()));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, credencial.IdEmpleado.ToString()),
            new(ClaimTypes.NameIdentifier, credencial.IdEmpleado.ToString()),
            new(ClaimTypes.Name, credencial.Usuario),
            new(ClaimTypes.Role, credencial.Rol!.Nombre),
            new("nombre_completo", $"{credencial.Nombre} {credencial.Apellido}".Trim()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(horas),
            signingCredentials: new SigningCredentials(claveFirma, SecurityAlgorithms.HmacSha256));

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiraEnHoras = horas,
            IdEmpleado = credencial.IdEmpleado,
            Nombre = credencial.Nombre,
            Apellido = credencial.Apellido,
            Usuario = credencial.Usuario,
            IdRol = credencial.IdRol,
            Rol = credencial.Rol!.Nombre
        };
    }

    private string _Clave()
    {
        var clave = _config["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(clave))
        {
            throw new InvalidOperationException(
                "Falta Jwt:SigningKey. Configuralo con: dotnet user-secrets set \"Jwt:SigningKey\" \"<clave>\" --project ELRINCONDORADO.API");
        }
        return clave;
    }
}

// Valida usuario y contrasena. El hash se compara en tiempo constante para no
// filtrar informacion por analisis de tiempos.
public class AutenticacionService
{
    private readonly AuthDbContext _db;
    private readonly TokenService _tokens;

    public AutenticacionService(AuthDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    public async Task<(LoginResponse? Respuesta, string? Error)> LoginAsync(
        string usuario, string password, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
        {
            return (null, "Usuario y contraseña son obligatorios.");
        }

        var credencial = await _db.Credenciales
            .AsNoTracking()
            .Include(e => e.Rol)
            .FirstOrDefaultAsync(e => e.Usuario == usuario.Trim(), ct);

        // Mismo mensaje para usuario inexistente y contraseña erronea: no se
        // revela que usuarios existen.
        const string fallo = "Usuario o contraseña incorrectos.";

        if (credencial is null)
        {
            return (null, fallo);
        }

        if (!Verificar(password, credencial.PasswordHash))
        {
            return (null, fallo);
        }

        if (!string.Equals(credencial.Estado, "ACTIVO", StringComparison.OrdinalIgnoreCase))
        {
            return (null, "El usuario está inactivo. Contacta al administrador.");
        }

        if (credencial.Rol is null)
        {
            return (null, "El usuario no tiene un rol asignado.");
        }

        return (_tokens.Emitir(credencial), null);
    }

    // Reproduce EXACTAMENTE el hash del proyecto MVC (SHA256 en Base64) para que
    // las 8 cuentas existentes puedan iniciar sesion en la API sin resetear nada.
    // TODO: migrar a PBKDF2/bcrypt con sal; exige resetear las contrasenas actuales.
    private static bool Verificar(string password, string hashAlmacenado)
    {
        var calculado = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(calculado),
            Encoding.UTF8.GetBytes(hashAlmacenado));
    }
}
