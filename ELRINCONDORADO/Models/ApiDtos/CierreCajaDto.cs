namespace ELRINCONDORADO.Models.ApiDtos;

public class CierreCajaDto
{
    public int IdCierre { get; set; }
    public DateTime Fecha { get; set; }
    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }
    public decimal TotalVentas { get; set; }
    public int MovimientosCerrados { get; set; }
    public bool EmpleadoDesconocido { get; set; }
}

public class CierreCajaDetalleDto : CierreCajaDto
{
    public DateTime? TurnoDesde { get; set; }
    public List<CierreCajaMovimientoResumen> Movimientos { get; set; } = new();
    public List<CierreCajaVentaDto> VentasIncluidas { get; set; } = new();
}

public class CierreCajaMovimientoResumen
{
    public string TipoMovimiento { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Unidades { get; set; }
}

public class CierreCajaVentaDto
{
    public int IdPedido { get; set; }
    public string? NombrePedido { get; set; }
    public string? NumeroPedido { get; set; }
    public string TipoPedido { get; set; } = string.Empty;
    public string? EstadoPago { get; set; }
    public DateTime FechaCreacion { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public MesaDto? Mesa { get; set; }
    public ClienteDto? Cliente { get; set; }
    public string? Estado { get; set; }
}

public class CierreCajaPaginadoDto
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<CierreCajaDto> Items { get; set; } = new();
}
