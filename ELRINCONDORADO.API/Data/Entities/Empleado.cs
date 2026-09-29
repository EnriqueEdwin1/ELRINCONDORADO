namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'empleados' de Supabase.
//
// IMPORTANTE: la columna 'password_hash' NO se mapea a proposito. Al no formar
// parte del modelo, EF Core no la incluye en el SELECT, de modo que esta API no
// puede leer hashes de contrasena ni por descuido. El hash sigue existiendo en
// Supabase y sigue a cargo del proyecto MVC / futura autenticacion por token.
public class Empleado
{
    public int IdEmpleado { get; set; }
    public int IdRol { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public DateTime? FechaContratacion { get; set; }
    public string Estado { get; set; } = string.Empty;

    public Rol? Rol { get; set; }
}
