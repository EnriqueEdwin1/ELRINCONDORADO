using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;
using ELRINCONDORADO.Services;
using ELRINCONDORADO.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ELRINCONDORADO.Controllers
{
    public class CocinaController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PantallaCocinaTracker _pantallasCocina;
        private readonly IHubContext<PedidosHub> _hubContext;

        public CocinaController(AppDbContext context, PantallaCocinaTracker pantallasCocina, IHubContext<PedidosHub> hubContext)
        {
            _context = context;
            _pantallasCocina = pantallasCocina;
            _hubContext = hubContext;
        }

        // Verifica que haya sesión activa y que el rol sea COCINERO
        private IActionResult? ValidarAcceso()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Rol") != "COCINERO")
                return StatusCode(403, "Solo el personal de cocina puede acceder a esta sección.");

            return null;
        }

        // Cola de cocina: los pedidos que la cocina tiene entre manos. Un pedido LISTO no sale de la
        // cola: se queda en verde esperando a que el mesero lo marque ENTREGADO (ver Mesero/Entregar),
        // que es cuando recién desaparece de esta pantalla.
        private static readonly string[] EstadosCola = { "PENDIENTE", "EN PREPARACION", "LISTO" };

        // Primero lo que aún no se tocó, después lo que está en preparación y al final lo listo.
        // Entre los listos va el más antiguo primero: es el que más lleva esperando al mesero.
        private static int OrdenCola(string estado) => estado switch
        {
            "PENDIENTE" => 0,
            "EN PREPARACION" => 1,
            _ => 2
        };

        // GET: Cocina -> cola de pedidos del POS (PENDIENTE, EN PREPARACION y LISTO) con detalle
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var pedidos = await _context.Pedidos
                .Include(p => p.Empleado)
                .Include(p => p.Mesa)
                .Include(p => p.DetallesPedidos)
                    .ThenInclude(d => d.Producto)
                .Where(p => EstadosCola.Contains(p.Estado))
                .ToListAsync();

            return View(OrdenarCola(pedidos));
        }

        private static List<Pedido> OrdenarCola(List<Pedido> pedidos) => pedidos
            .OrderBy(p => OrdenCola(p.Estado))
            .ThenBy(p => p.Estado == "LISTO" ? p.FechaCreacion : DateTime.MaxValue)
            .ThenByDescending(p => p.FechaCreacion)
            .ToList();

        // GET: Cocina/ActualizarCola -> JSON de la cola activa para la pantalla en vivo
        // (la vista lo consulta en intervalos: anuncia por voz los pedidos PENDIENTE nuevos)
        [HttpGet]
        public async Task<IActionResult> ActualizarCola()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var pedidos = await _context.Pedidos
                .Where(p => EstadosCola.Contains(p.Estado))
                .Select(p => new
                {
                    p.IdPedido,
                    p.Estado,
                    p.TipoPedido,
                    MesaNumero = p.Mesa != null ? (int?)p.Mesa.Numero : null,
                    p.NombrePedido,
                    NombreCajero = p.Empleado != null
                        ? (p.Empleado.Nombre + " " + p.Empleado.Apellido).Trim()
                        : "",
                    p.FechaCreacion,
                    Total = p.Total > 0 ? p.Total : (p.Subtotal - p.Descuento),
                    p.EstadoPago,
                    Detalle = p.DetallesPedidos.Select(d => new
                    {
                        d.Cantidad,
                        Nombre = d.Producto != null ? d.Producto.Nombre : "Producto",
                        d.Observacion
                    })
                })
                .ToListAsync();

            var ordenados = pedidos
                .OrderBy(p => OrdenCola(p.Estado))
                .ThenBy(p => p.Estado == "LISTO" ? p.FechaCreacion : DateTime.MaxValue)
                .ThenByDescending(p => p.FechaCreacion)
                .ToList();

            return Json(ordenados);
        }

        // GET: Cocina/Ping -> la pantalla de cocina avisa que sigue abierta. El aviso sonoro solo
        // se emite si esta pantalla está en uso, para que no suene en otras pantallas de la app.
        [HttpGet]
        public IActionResult Ping()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            _pantallasCocina.Registrar(HttpContext.Session.Id);
            return NoContent();
        }

        // POST: Cocina/CambiarEstado -> avanza el pedido a EN PREPARACION o LISTO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string estado)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            // Un pedido cancelado por el administrador es definitivo: la cocina no lo revive.
            if (pedido.Estado == "CANCELADO")
            {
                TempData["Error"] = $"El pedido #{pedido.IdPedido} está cancelado y no se puede cambiar su estado.";
                return RedirectToAction(nameof(Index));
            }

            var nuevoEstado = estado?.Trim().ToUpperInvariant();
            if (nuevoEstado != "EN PREPARACION" && nuevoEstado != "LISTO")
            {
                TempData["Error"] = "Estado de cocina inválido.";
                return RedirectToAction(nameof(Index));
            }

            var estadoAnterior = pedido.Estado;
            pedido.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            // Notificar el cambio de estado en tiempo real mediante SignalR
            await _hubContext.Clients.Group("Cocina").SendAsync("RecibirCambioEstado", pedido.IdPedido, nuevoEstado);
            await _hubContext.Clients.Group("Cajero").SendAsync("RecibirCambioEstado", pedido.IdPedido, nuevoEstado);
            
            // Si el pedido está LISTO, notificar también al mesero
            if (nuevoEstado == "LISTO")
            {
                await _hubContext.Clients.Group("Mesero").SendAsync("RecibirCambioEstado", pedido.IdPedido, nuevoEstado);
            }

            TempData["Exito"] = nuevoEstado == "LISTO"
                ? $"Pedido #{pedido.IdPedido} marcado como LISTO. Avisa al mesero con la campana."
                : estadoAnterior == "LISTO"
                    ? $"Pedido #{pedido.IdPedido} volvió a preparación: ya no está en la lista de listos."
                    : $"Pedido #{pedido.IdPedido} ahora EN PREPARACION.";
            return RedirectToAction(nameof(Index));
        }
    }
}