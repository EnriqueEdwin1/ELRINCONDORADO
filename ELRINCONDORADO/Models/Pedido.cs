namespace ELRINCONDORADO.Models
{
    public class Pedido
    {
        public int IdPedido { get; set; }
        public int? IdMesa { get; set; }
        public int IdEmpleado { get; set; }
        public int? IdPromocion { get; set; }
        public string TipoPedido { get; set; }
        public string Estado { get; set; } = "PENDIENTE";
        public string EstadoPago { get; set; } = "PENDIENTE";
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public decimal Subtotal { get; set; } = 0;
        public decimal Descuento { get; set; } = 0;
        public decimal Total { get; set; } = 0;

        // Nombre del pedido/cliente que se muestra en cocina y tickets
        public string? NombrePedido { get; set; }

        // Número de factura consecutivo (se reinicia en 1 en cada cierre de caja)
        public string? NumeroPedido { get; set; }

        // Datos de facturación (el NIT/razón social viven en la tabla clientes, vinculada por IdCliente)
        public int? IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        public string? Observaciones { get; set; }

        // Relaciones
        public Mesa? Mesa { get; set; }
        public Empleado? Empleado { get; set; }
        public Promocion? Promocion { get; set; }
        public ICollection<DetallePedido>? DetallesPedidos { get; set; }
    }
}
