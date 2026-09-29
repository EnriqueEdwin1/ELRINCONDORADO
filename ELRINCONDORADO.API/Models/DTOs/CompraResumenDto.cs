namespace ELRINCONDORADO.API.Models.DTOs;

// Totales de compras a proveedores. Se agrega en cada llamada, no es una tabla.
public class CompraResumenDto
{
    public int TotalCompras { get; set; }
    public decimal TotalSubtotal { get; set; }
    public decimal TotalDescuento { get; set; }
    public decimal TotalComprado { get; set; }

    // Promedio por compra.
    public decimal CompraPromedio { get; set; }

    public List<CompraResumenAgrupado> PorProveedor { get; set; } = new();
    public List<CompraResumenDia> PorDia { get; set; } = new();
}

// Totales por proveedor.
public class CompraResumenAgrupado
{
    public int IdProveedor { get; set; }
    public string? ProveedorNombre { get; set; }
    public int Compras { get; set; }
    public decimal Total { get; set; }
}

// Totales de un día, con la fecha en formato yyyy-MM-dd.
public class CompraResumenDia
{
    public DateOnly Fecha { get; set; }
    public int Compras { get; set; }
    public decimal Total { get; set; }
}
