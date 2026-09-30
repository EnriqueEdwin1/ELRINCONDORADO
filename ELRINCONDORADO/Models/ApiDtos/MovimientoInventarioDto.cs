namespace ELRINCONDORADO.Models.ApiDtos;

public class MovimientoInventarioDto
{
    public int IdMovimiento { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public DateTime Fecha { get; set; }
    public string? Motivo { get; set; }
    public int IdInsumo { get; set; }
    public string? InsumoNombre { get; set; }
    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }
    public int? IdCierre { get; set; }
}
