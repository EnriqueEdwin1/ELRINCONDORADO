namespace ELRINCONDORADO.Models
{
    public class DetallePromocion
    {
        public int IdDetallePromocion { get; set; }
        public int IdPromocion { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }

        // Relaciones
        public Promocion? Promocion { get; set; }
        public Producto? Producto { get; set; }
    }
}
