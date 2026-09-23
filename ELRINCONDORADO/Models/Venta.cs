namespace ELRINCONDORADO.Models
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public int? IdMesa { get; set; }
        public int IdEmpleado { get; set; }
        public DateTime FechaVenta { get; set; } = DateTime.UtcNow;
        public decimal Subtotal { get; set; } = 0;
        public decimal Descuento { get; set; } = 0;
        public decimal Total { get; set; } = 0;
        public string MetodoPago { get; set; }
        public string Estado { get; set; } = "PAGADA";
        public string? Observaciones { get; set; }

        // Relaciones
        public Mesa? Mesa { get; set; }
        public Empleado? Empleado { get; set; }
    }
}
