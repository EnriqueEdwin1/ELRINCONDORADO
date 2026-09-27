namespace ELRINCONDORADO.Models
{
    /// <summary>Insumo que está por debajo del stock mínimo: hay que comprarlo.</summary>
    public class InsumoBajoMinimoViewModel
    {
        public int IdInsumo { get; set; }
        public string Nombre { get; set; } = "";
        public string UnidadMedida { get; set; } = "";
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }

        /// <summary>Cuánto falta para llegar al mínimo.</summary>
        public decimal Faltante => Math.Max(0, StockMinimo - StockActual);
    }
}
