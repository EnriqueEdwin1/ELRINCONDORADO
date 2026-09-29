namespace ELRINCONDORADO.API.Models.DTOs;

// Contrato de salida del catálogo. 'DeleteUrl' no aparece a propósito: es el token
// de borrado de ImgBB y exponerlo permitiría que cualquiera borrara las imágenes
// del catálogo desde ImgBB.
public class ProductoDto
{
    public int IdProducto { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
    public string? ImagenUrl { get; set; }
    public string? DisplayUrl { get; set; }

    public int IdCategoria { get; set; }
    public string? CategoriaNombre { get; set; }

    // Basta un booleano para el listado: así no se carga la receta entera
    // (y sus insumos) en cada fila.
    public bool TieneReceta { get; set; }
}

// Detalle del producto, que además trae la receta con sus insumos.
public class ProductoDetalleDto : ProductoDto
{
    public RecetaDto? Receta { get; set; }
}
