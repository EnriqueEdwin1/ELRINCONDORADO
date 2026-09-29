using Microsoft.AspNetCore.SignalR;

namespace ELRINCONDORADO.API.Hubs
{
    public class PedidosHub : Hub
    {
        public async Task NotificarNuevoPedido(int idPedido)
        {
            await Clients.Group("Cocina").SendAsync("RecibirNuevoPedido", idPedido);
        }

        public async Task NotificarCambioEstadoPedido(int idPedido, string estado)
        {
            await Clients.Group("Cocina").SendAsync("RecibirCambioEstado", idPedido, estado);
            await Clients.Group("Cajero").SendAsync("RecibirCambioEstado", idPedido, estado);
            
            // Si el pedido está LISTO, notificar también al mesero
            if (estado == "LISTO")
            {
                await Clients.Group("Mesero").SendAsync("RecibirCambioEstado", idPedido, estado);
            }
        }

        public async Task UnirseACocina()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Cocina");
        }

        public async Task UnirseACajero()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Cajero");
        }

        public async Task SalirDeCocina()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Cocina");
        }

        public async Task SalirDeCajero()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Cajero");
        }

        public async Task UnirseAMesero()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Mesero");
        }

        public async Task SalirDeMesero()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Mesero");
        }
    }
}
