namespace ELRINCONDORADO.Models
{
    public class Compra
    {
        public int IdCompra { get; set; }
        public int IdProveedor { get; set; }
        public int IdEmpleado { get; set; }
        public DateTime FechaCompra { get; set; } = DateTime.UtcNow;
        public decimal Subtotal { get; set; } = 0;
        public decimal Descuento { get; set; } = 0;
        public decimal Total { get; set; } = 0;
        public string Estado { get; set; } = "REGISTRADA";

        // Relaciones
        public Proveedor? Proveedor { get; set; }
        public Empleado? Empleado { get; set; }
        public ICollection<DetalleCompra>? DetallesCompras { get; set; }
    }
}
