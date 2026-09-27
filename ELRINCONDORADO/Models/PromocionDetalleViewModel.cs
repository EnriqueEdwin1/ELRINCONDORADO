namespace ELRINCONDORADO.Models
{
    /// <summary>
    /// Detalle de una promoción: qué productos incluye (detalle_promociones), cuánto valen
    /// por separado y cuánto se ahorra con el precio de la promoción.
    /// </summary>
    public class PromocionDetalleViewModel
    {
        public Promocion Promocion { get; set; } = new Promocion();

        public List<ProductoPromocionItem> Incluidos { get; set; } = new List<ProductoPromocionItem>();

        public decimal SumaProductos => Incluidos.Sum(i => i.Precio * i.Cantidad);

        public decimal Precio => Promocion.Valor;

        public decimal Ahorro => SumaProductos - Precio;

        public decimal PorcentajeAhorro
        {
            get
            {
                var suma = SumaProductos;
                if (suma <= 0 || Ahorro <= 0) return 0;
                return Math.Round(Ahorro / suma * 100, 1);
            }
        }

        public bool TieneAhorro => Ahorro > 0;

        public string ResumenProductos => string.Join(", ",
            Incluidos.Select(i => $"{i.Nombre} x{i.Cantidad}"));
    }
}
