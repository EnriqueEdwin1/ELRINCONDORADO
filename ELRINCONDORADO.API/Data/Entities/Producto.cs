namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'productos' de Supabase.
//
// IMPORTANTE: la columna 'delete_url' NO se mapea a propósito. Contiene el token
// de borrado de ImgBB: quien lo posee puede eliminar la imagen del hosting. Al no
// formar parte del modelo, EF Core no la incluye en el SELECT y esta API no
// puede devolverla ni por descuido. Sigue en Supabase a cargo del proyecto MVC,
// que la usa para borrar imágenes desde el panel de administración.
public class Producto
{
    public int IdProducto { get; set; }
    public int IdCategoria { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    // numeric(10,2) en Supabase
    public decimal Precio { get; set; }

    public string? ImagenUrl { get; set; }
    public string? DisplayUrl { get; set; }
    public bool Activo { get; set; }

    public Categoria? Categoria { get; set; }
    public Receta? Receta { get; set; }
}
