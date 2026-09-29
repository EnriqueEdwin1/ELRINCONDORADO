namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'mesas' de Supabase.
public class Mesa
{
    public int IdMesa { get; set; }
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public string Estado { get; set; } = string.Empty;
}
