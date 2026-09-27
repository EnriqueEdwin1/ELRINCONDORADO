namespace ELRINCONDORADO.Models
{
    /// <summary>
    /// Datos de la pantalla de Ventas: los pedidos facturados (la tabla ventas se eliminó,
    /// todo sale de pedidos + detalle_pedidos) con el total del período filtrado.
    /// </summary>
    public class VentasListaViewModel
    {
        public List<Pedido> Pedidos { get; set; } = new();
        public int Cantidad => Pedidos.Count;

        /// <summary>Pedidos que cuentan como venta (un pedido cancelado no factura).</summary>
        public IEnumerable<Pedido> Facturados => Pedidos.Where(p => p.Estado != "CANCELADO");

        public int Cancelados => Pedidos.Count(p => p.Estado == "CANCELADO");

        /// <summary>Suma de los pedidos facturados del período (subtotal y total con descuentos).</summary>
        public decimal Subtotal => Facturados.Sum(p => p.Subtotal);

        public decimal Total => Facturados.Sum(p => p.Total);

        public decimal Descuento => Facturados.Sum(p => p.Descuento);

        /// <summary>Rango de fechas aplicado; null = todos los días.</summary>
        public DateTime? Desde { get; set; }

        public DateTime? Hasta { get; set; }

        /// <summary>Texto del período para los encabezados ("Hoy", "Últimos 7 días", "Todo").</summary>
        public string Periodo { get; set; } = "Todo";
    }
}
