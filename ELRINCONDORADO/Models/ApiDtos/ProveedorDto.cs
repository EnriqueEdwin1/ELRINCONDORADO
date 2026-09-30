namespace ELRINCONDORADO.Models.ApiDtos;

public class ProveedorDto
{
    public int IdProveedor { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Email { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public int TotalCompras { get; set; }
    public decimal TotalGastado { get; set; }
    public DateTime? UltimaCompra { get; set; }
}
