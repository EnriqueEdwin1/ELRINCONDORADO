namespace ELRINCONDORADO.Models
{
    public class DetallePedido
    {
        public int IdDetalle { get; set; }
        public int IdPedido { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string? Observacion { get; set; }

        // Relaciones
        public Pedido? Pedido { get; set; }
        public Producto? Producto { get; set; }
    }
}
