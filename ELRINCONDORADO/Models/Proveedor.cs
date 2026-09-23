namespace ELRINCONDORADO.Models
{
    public class Proveedor
    {
        public int IdProveedor { get; set; }
        public string Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Email { get; set; }
        public string? Observaciones { get; set; }
        public string Estado { get; set; } = "ACTIVO";

        // Relaciones
        public ICollection<Compra>? Compras { get; set; }
    }
}
