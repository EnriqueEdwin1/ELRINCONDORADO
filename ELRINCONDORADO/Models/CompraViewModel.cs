namespace ELRINCONDORADO.Models
{
    public class CompraViewModel
    {
        public int IdProveedor { get; set; }
        public decimal Descuento { get; set; } = 0;
        public List<DetalleCompraViewModel> Detalles { get; set; } = new();

        public decimal Subtotal => Detalles.Sum(d => d.Subtotal);
        public decimal Total => Subtotal - Descuento;
    }

    public class DetalleCompraViewModel
    {
        public int IdInsumo { get; set; }
        public decimal Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }

        public decimal Subtotal => Cantidad * CostoUnitario;
    }
}