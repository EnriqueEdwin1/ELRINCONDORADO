namespace ELRINCONDORADO.Models
{
    public class CierreCajaViewModel
    {
        // Ventas (pedidos facturados) del día
        public List<Pedido> Ventas { get; set; } = new();

        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public decimal Efectivo { get; set; }
        public decimal Tarjeta { get; set; }
        public decimal QR { get; set; }

        // Cajero que cierra la caja y fecha del cierre
        public string Cajero { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}