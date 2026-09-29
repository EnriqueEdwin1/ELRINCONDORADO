namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'clientes' de Supabase.
// Datos de facturación: NIT y razón social. Cada pedido puede vincularse a un
// cliente mediante pedidos.id_cliente.
//
// Nota de privacidad: el NIT es un dato tributario de una persona o empresa. Se
// expone porque los tickets lo necesitan, pero conviene protegerlo antes de
// publicar la API fuera de la red interna. Ver Controllers/PedidosController.cs.
public class Cliente
{
    public int IdCliente { get; set; }

    // varchar(20) en Supabase.
    public string Nit { get; set; } = string.Empty;

    // varchar(150) en Supabase.
    public string RazonSocial { get; set; } = string.Empty;

    public ICollection<Pedido>? Pedidos { get; set; }
}
