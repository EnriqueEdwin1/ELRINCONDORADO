namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'roles' de Supabase.
public class Rol
{
    public int IdRol { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
