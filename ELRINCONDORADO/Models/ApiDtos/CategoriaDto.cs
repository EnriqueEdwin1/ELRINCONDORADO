namespace ELRINCONDORADO.Models.ApiDtos;

public class CategoriaDto
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CantidadProductos { get; set; }
    public List<ProductoDto> Productos { get; set; } = new();
}
