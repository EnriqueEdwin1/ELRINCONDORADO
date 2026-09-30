namespace ELRINCONDORADO.Models.ApiDtos;

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
    public bool TieneReceta { get; set; }
}

public class ProductoDetalleDto : ProductoDto
{
    public RecetaDto? Receta { get; set; }
}

public class RecetaDto
{
    public int IdReceta { get; set; }
    public int IdProducto { get; set; }
    public string? ProductoNombre { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public List<RecetaDetalleDto> Detalles { get; set; } = new();
}

public class RecetaDetalleDto
{
    public int IdDetalleReceta { get; set; }
    public int IdInsumo { get; set; }
    public string? InsumoNombre { get; set; }
    public decimal Cantidad { get; set; }
    public string? UnidadMedida { get; set; }
}
