namespace ELRINCONDORADO.Models
{
    public class RecetaViewModel
    {
        public int IdReceta { get; set; }
        public int IdProducto { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;

        public List<DetalleRecetaViewModel> Detalles { get; set; } = new List<DetalleRecetaViewModel>();
    }

    public class DetalleRecetaViewModel
    {
        public int IdDetalleReceta { get; set; }
        public int IdInsumo { get; set; }
        public decimal Cantidad { get; set; }
        public string? UnidadMedida { get; set; }
    }
}
