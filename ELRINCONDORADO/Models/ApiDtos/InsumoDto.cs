namespace ELRINCONDORADO.Models.ApiDtos;

public class InsumoDto
{
    public int IdInsumo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal CostoUnitario { get; set; }
    public bool Activo { get; set; }
    public int? IdDestino { get; set; }
    public string? DestinoNombre { get; set; }
    public bool BajoMinimo { get; set; }
    public decimal Faltante => StockMinimo > 0 ? Math.Max(0, StockMinimo - StockActual) : 0;
}
