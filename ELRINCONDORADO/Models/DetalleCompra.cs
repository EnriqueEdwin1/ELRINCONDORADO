namespace ELRINCONDORADO.Models
{
    public class DetalleCompra
    {
        public int IdDetalleCompra { get; set; }
        public int IdCompra { get; set; }
        public int IdInsumo { get; set; }
        public decimal Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Subtotal { get; set; }

        // Relaciones
        public Compra? Compra { get; set; }
        public Insumo? Insumo { get; set; }
    }
}
