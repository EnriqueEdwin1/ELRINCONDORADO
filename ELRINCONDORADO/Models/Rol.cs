namespace ELRINCONDORADO.Models
{
    public class Rol
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }

        // Relaciones
        public ICollection<Empleado>? Empleados { get; set; }
    }
}
