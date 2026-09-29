namespace ELRINCONDORADO.API.Models.DTOs;

// Contrato de salida de la API. Se separa de la entidad de EF Core para que el
// esquema de la base de datos y la forma de la respuesta sean independientes.
public class CategoriaDto
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CantidadProductos { get; set; }
}
