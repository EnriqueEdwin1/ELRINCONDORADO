namespace ELRINCONDORADO.Models
{
    public class Mesa
    {
        public int IdMesa { get; set; }
        public int Numero { get; set; }
        public int Capacidad { get; set; }
        public string Estado { get; set; } = "DISPONIBLE";

        // Relaciones
        public ICollection<Pedido>? Pedidos { get; set; }
        public ICollection<Venta>? Ventas { get; set; }
    }
}
