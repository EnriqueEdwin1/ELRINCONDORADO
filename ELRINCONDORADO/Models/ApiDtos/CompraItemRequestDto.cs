namespace ELRINCONDORADO.Models.ApiDtos;

public class CompraItemRequestDto
{
    public int IdInsumo { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
