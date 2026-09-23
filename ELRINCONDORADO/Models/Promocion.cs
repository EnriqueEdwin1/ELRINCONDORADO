namespace ELRINCONDORADO.Models
{
    public class Promocion
    {
        public int IdPromocion { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string Tipo { get; set; }
        public decimal Valor { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public TimeSpan? HoraInicio { get; set; }
        public TimeSpan? HoraFin { get; set; }
        public string Estado { get; set; } = "ACTIVA";

        // Relaciones
        public ICollection<DetallePromocion>? DetallePromociones { get; set; }
        public ICollection<Pedido>? Pedidos { get; set; }
    }
}
