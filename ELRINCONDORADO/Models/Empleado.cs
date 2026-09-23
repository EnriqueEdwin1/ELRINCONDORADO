namespace ELRINCONDORADO.Models
{
    public class Empleado
    {
        public int IdEmpleado { get; set; }
        public int IdRol { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Usuario { get; set; }
        public string PasswordHash { get; set; }
        public string? Telefono { get; set; }
        public DateTime? FechaContratacion { get; set; }
        public string Estado { get; set; } = "ACTIVO";

        // Relaciones
        public Rol? Rol { get; set; }
        public ICollection<Pedido>? Pedidos { get; set; }
        public ICollection<Venta>? Ventas { get; set; }
        public ICollection<Compra>? Compras { get; set; }
        public ICollection<MovimientoInventario>? MovimientosInventario { get; set; }
    }
}
