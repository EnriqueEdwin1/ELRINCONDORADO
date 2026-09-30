namespace ELRINCONDORADO.Models.ApiDtos;

// DTO para crear compras con sus detalles
public class CompraConDetallesDto : CompraDto
{
    public List<CompraDetalleRequestDto> Detalles { get; set; } = new();
}

public class CompraDetalleRequestDto
{
    public int IdInsumo { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
