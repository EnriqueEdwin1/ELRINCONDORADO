namespace ELRINCONDORADO.API.Models.DTOs;

// Receta de un producto: descripción y lista de insumos que consume.
public class RecetaDto
{
    public int IdReceta { get; set; }
    public int IdProducto { get; set; }
    public string? ProductoNombre { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public List<RecetaDetalleDto> Detalles { get; set; } = new();
}

// Un insumo dentro de una receta.
public class RecetaDetalleDto
{
    public int IdDetalleReceta { get; set; }
    public int IdInsumo { get; set; }
    public string? InsumoNombre { get; set; }
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
}
