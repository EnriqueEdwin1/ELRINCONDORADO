namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'compras' de Supabase.
// Cabecera de una compra a proveedor. Las líneas viven en detalle_compras.
//
// 'estado' es varchar y hoy solo tiene 'REGISTRADA'. No hay catálogo que lo
// valide, igual que 'estado_pago' de pedidos.
//
// 'fecha_compra' es 'timestamp without time zone' y guarda hora local, igual que
// pedidos.fecha_creacion. No convertir a UTC: correría las compras de la tarde.
public class Compra
{
    public int IdCompra { get; set; }
    public int IdProveedor { get; set; }
    public int IdEmpleado { get; set; }

    public DateTime FechaCompra { get; set; }

    // Los tres importes son numeric(10,2) en Supabase.
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }

    // REGISTRADA (único valor en producción)
    public string Estado { get; set; } = string.Empty;

    public Proveedor? Proveedor { get; set; }
    public Empleado? Empleado { get; set; }
    public ICollection<DetalleCompra>? DetalleCompras { get; set; }
}
