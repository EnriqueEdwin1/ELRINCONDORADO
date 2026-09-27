namespace ELRINCONDORADO.Models
{
    public class FacturaViewModel
    {
        public string NombrePedido { get; set; }
        public string? Nit { get; set; }
        public string? RazonSocial { get; set; }
        public string? TipoPedido { get; set; }
        public int? IdMesa { get; set; }
        public string? MetodoPago { get; set; }
        public string? Observaciones { get; set; }
        public List<FacturaItemViewModel> Items { get; set; } = new List<FacturaItemViewModel>();
    }

    public class FacturaItemViewModel
    {
        public int IdProducto { get; set; }

        /// <summary>Si es mayor a cero, la línea es una promoción y IdProducto se ignora.</summary>
        public int IdPromocion { get; set; }

        public int Cantidad { get; set; }
        public string? Observacion { get; set; }
    }
}
