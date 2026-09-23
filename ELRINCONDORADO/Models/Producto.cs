namespace ELRINCONDORADO.Models
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public int IdCategoria { get; set; }
        public string? Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string? ImagenUrl { get; set; }
        public string? DisplayUrl { get; set; }
        public string? DeleteUrl { get; set; }
        public bool Activo { get; set; } = true;

        // Relaciones
        public Categoria? Categoria { get; set; }
        public Receta? Receta { get; set; }
        public ICollection<DetallePedido>? DetallesPedidos { get; set; }
        public ICollection<DetallePromocion>? DetallePromociones { get; set; }
    }
}
