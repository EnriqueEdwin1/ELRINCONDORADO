namespace ELRINCONDORADO.Models
{
    public class Insumo
    {
        public int IdInsumo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string UnidadMedida { get; set; }
        public decimal StockActual { get; set; } = 0;
        public decimal StockMinimo { get; set; } = 0;
        public decimal CostoUnitario { get; set; } = 0;
        public bool Activo { get; set; } = true;
        public int IdDestino { get; set; } = 1;

        // Relaciones
        public DestinoInsumo? Destino { get; set; }
        public ICollection<DetalleReceta>? DetalleRecetas { get; set; }
        public ICollection<DetalleCompra>? DetallesCompras { get; set; }
        public ICollection<MovimientoInventario>? MovimientosInventario { get; set; }
    }
}
