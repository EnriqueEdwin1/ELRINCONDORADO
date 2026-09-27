namespace ELRINCONDORADO.Models
{
    public class MovimientosAdminViewModel
    {
        public List<MovimientoInventario> TurnoActual { get; set; } = new();
        public List<CierreCaja> Cierres { get; set; } = new();
        public int CantidadTurnoActual { get; set; }
    }
}