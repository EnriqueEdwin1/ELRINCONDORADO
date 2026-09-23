namespace ELRINCONDORADO.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string Nit { get; set; } = "";
        public string RazonSocial { get; set; } = "";

        // Relación 1:N con pedidos (cada pedido factura a un cliente)
        public ICollection<Pedido>? Pedidos { get; set; }
    }
}
