namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'categorias' de Supabase.
// No se creó ni se alteró la tabla: el mapeo solo sirve para LEERLA.
public class Categoria
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
