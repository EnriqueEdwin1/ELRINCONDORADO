namespace ELRINCONDORADO.Models
{
    /// <summary>
    /// Fila de la lista de productos que se pueden incluir en una promoción.
    /// El formulario marca cuáles están seleccionados y en qué cantidad.
    /// </summary>
    public class ProductoPromocionItem
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public bool Activo { get; set; } = true;
        public bool Seleccionado { get; set; }
        public int Cantidad { get; set; } = 1;
    }
}
