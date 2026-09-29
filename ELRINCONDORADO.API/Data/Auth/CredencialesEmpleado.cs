namespace ELRINCONDORADO.API.Data.Auth;

// Mapeo AISLADO de la tabla 'empleados' usado EXCLUSIVAMENTE por el login.
//
// Se separa de Data/Entities/Empleado.cs a proposito: la entidad que alimenta los
// DTOs publicos no incluye 'password_hash', mientras que esta si. Asi ningun
// endpoint de listado o detalle puede leer un hash por accidente, y el unico
// punto de entrada que toca la columna es el servicio de autenticacion.
public class CredencialesEmpleado
{
    public int IdEmpleado { get; set; }
    public int IdRol { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;

    public Entities.Rol? Rol { get; set; }
}
