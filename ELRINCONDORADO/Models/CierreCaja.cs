namespace ELRINCONDORADO.Models
{
    public class CierreCaja
    {
        public int IdCierre { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public int IdEmpleado { get; set; }
        public decimal TotalVentas { get; set; } = 0;

        public Empleado? Empleado { get; set; }
        public ICollection<MovimientoInventario>? Movimientos { get; set; }
    }
}