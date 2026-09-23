namespace ELRINCONDORADO.Models
{
    public class MovimientoInventario
    {
        public int IdMovimiento { get; set; }
        public int IdInsumo { get; set; }
        public int IdEmpleado { get; set; }
        public string TipoMovimiento { get; set; }
        public decimal Cantidad { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public string? Motivo { get; set; }

        // Relaciones
        public Insumo? Insumo { get; set; }
        public Empleado? Empleado { get; set; }
    }
}
