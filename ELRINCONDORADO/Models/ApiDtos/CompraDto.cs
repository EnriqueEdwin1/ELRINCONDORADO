namespace ELRINCONDORADO.Models.ApiDtos;

public class CompraDto
{
    public int IdCompra { get; set; }
    public int IdProveedor { get; set; }
    public string? ProveedorNombre { get; set; }
    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }
    public DateTime FechaCompra { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int Lineas { get; set; }
    public decimal SumaDetalle { get; set; }
    public bool SubtotalConsistente { get; set; }
}

public class CompraDetalleDto : CompraDto
{
    public List<CompraItemDto> Items { get; set; } = new();
    public int LineasConEntradaRegistrada { get; set; }
    public int LineasTotales { get; set; }
    public bool InventarioCompleto { get; set; }
}

public class CompraItemDto
{
    public int IdDetalleCompra { get; set; }
    public int IdInsumo { get; set; }
    public string? InsumoNombre { get; set; }
    public bool InsumoActivo { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public decimal? CostoUnitarioActual { get; set; }
    public bool CostoCambioDesdeCompra { get; set; }
    public bool EntradaRegistrada { get; set; }
}

public class CompraPaginadoDto
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<CompraDto> Items { get; set; } = new();
}

public class CompraResumenDto
{
    public int TotalCompras { get; set; }
    public decimal TotalSubtotal { get; set; }
    public decimal TotalDescuento { get; set; }
    public decimal TotalComprado { get; set; }
    public decimal CompraPromedio { get; set; }
    public List<CompraResumenAgrupado> PorProveedor { get; set; } = new();
    public List<CompraResumenDia> PorDia { get; set; } = new();
}

public class CompraResumenAgrupado
{
    public int IdProveedor { get; set; }
    public string? ProveedorNombre { get; set; }
    public int Compras { get; set; }
    public decimal Total { get; set; }
}

public class CompraResumenDia
{
    public DateOnly Fecha { get; set; }
    public int Compras { get; set; }
    public decimal Total { get; set; }
}
