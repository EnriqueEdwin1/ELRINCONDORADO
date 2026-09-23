namespace ELRINCONDORADO.Models
{
    public class Receta
    {
        public int IdReceta { get; set; }
        public int IdProducto { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;

        // Relaciones
        public Producto? Producto { get; set; }
        public ICollection<DetalleReceta>? DetalleRecetas { get; set; }
    }
}