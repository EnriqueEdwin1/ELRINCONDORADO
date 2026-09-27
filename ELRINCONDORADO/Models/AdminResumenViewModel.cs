namespace ELRINCONDORADO.Models
{
    /// <summary>
    /// Cuadro de resumen del panel de administración: qué insumos están bajo el mínimo y
    /// cuánto se lleva facturado en el turno actual (se reinicia con el cierre de caja).
    /// </summary>
    public class AdminResumenViewModel
    {
        public List<InsumoBajoMinimoViewModel> InsumosBajoMinimo { get; set; } = new List<InsumoBajoMinimoViewModel>();

        // Turno actual: todo lo facturado después del último cierre de caja
        public decimal VendidoTurno { get; set; }
        public int VentasTurno { get; set; }
        public DateTime? UltimoCierre { get; set; }

        // Referencia del día (no se reinicia con el cierre)
        public decimal VendidoHoy { get; set; }
        public int VentasHoy { get; set; }

        public DateTime Ahora => DateTime.Now;

        public DateTime? UltimoCierreLocal => UltimoCierre?.ToLocalTime();
    }
}
