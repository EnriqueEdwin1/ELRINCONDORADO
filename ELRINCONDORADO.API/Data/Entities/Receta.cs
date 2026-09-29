namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'recetas' de Supabase.
// La receta es 1:1 opcional con un producto: describe cómo se prepara.
public class Receta
{
    public int IdReceta { get; set; }
    public int IdProducto { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }

    public Producto? Producto { get; set; }
    public ICollection<DetalleReceta>? DetalleRecetas { get; set; }
}
