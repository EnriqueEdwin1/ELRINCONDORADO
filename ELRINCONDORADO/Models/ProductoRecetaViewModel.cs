namespace ELRINCONDORADO.Models
{
    public class ProductoRecetaViewModel
    {
        public Producto Producto { get; set; } = new Producto { Nombre = "" };

        // Indica si el formulario debe crear/actualizar la receta del producto
        public bool IncluirReceta { get; set; }

        public RecetaViewModel Receta { get; set; } = new RecetaViewModel();
    }
}