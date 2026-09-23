namespace ELRINCONDORADO.Models
{
    public class DetalleReceta
    {
        public int IdDetalleReceta { get; set; }
        public int IdReceta { get; set; }
        public int IdInsumo { get; set; }
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; }

        // Relaciones
        public Receta? Receta { get; set; }
        public Insumo? Insumo { get; set; }
    }
}
