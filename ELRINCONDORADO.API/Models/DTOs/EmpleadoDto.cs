namespace ELRINCONDORADO.API.Models.DTOs;

// No incluye PasswordHash a proposito: el DTO de respuesta jamas expone el hash.
public class EmpleadoDto
{
    public int IdEmpleado { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public DateTime? FechaContratacion { get; set; }
    public string Estado { get; set; } = string.Empty;

    // Datos del rol ya aplanados, equivalente al Include(e => e.Rol) del MVC.
    public int IdRol { get; set; }
    public string RolNombre { get; set; } = string.Empty;
}
