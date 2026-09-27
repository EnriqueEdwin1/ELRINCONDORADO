namespace ELRINCONDORADO.Models
{
    /// <summary>
    /// Fila del listado de promociones: la promoción (tabla promociones), lo que valen
    /// sus productos por separado y cuánto se ahorra con el precio de la promoción.
    /// </summary>
    public class PromocionListadoViewModel
    {
        public Promocion Promocion { get; set; } = new Promocion();

        public decimal SumaProductos { get; set; }

        public int CantidadProductos { get; set; }

        /// <summary>Ejemplo: "Hamburguesa x1, Papa frita x2".</summary>
        public string ResumenProductos { get; set; } = "";

        public decimal Precio => Promocion.Valor;

        public decimal Ahorro => SumaProductos - Precio;

        public decimal PorcentajeAhorro
        {
            get
            {
                if (SumaProductos <= 0 || Ahorro <= 0) return 0;
                return Math.Round(Ahorro / SumaProductos * 100, 1);
            }
        }
    }
}
