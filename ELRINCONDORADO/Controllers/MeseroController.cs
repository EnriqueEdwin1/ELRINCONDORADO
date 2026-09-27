using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;
using ELRINCONDORADO.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ELRINCONDORADO.Controllers
{
    public class MeseroController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<PedidosHub> _hubContext;

        public MeseroController(AppDbContext context, IHubContext<PedidosHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Verifica que haya sesión activa y que el rol sea MESERO
        private IActionResult? ValidarAcceso()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Rol") != "MESERO")
                return StatusCode(403, "Solo el mesero puede acceder a esta sección.");

            return null;
        }

        // GET: Mesero -> pedidos en estado LISTO (cocina terminó) para servir a la mesa
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var pedidos = await _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Empleado)
                .Include(p => p.DetallesPedidos)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.Estado == "LISTO")
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            return View(pedidos);
        }

        // POST: Mesero/Entregar -> el mesero lleva el pedido a la mesa y lo marca ENTREGADO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entregar(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            // Un pedido cancelado por el administrador es definitivo: el mesero no lo revive.
            if (pedido.Estado == "CANCELADO")
            {
                TempData["Exito"] = $"El pedido #{pedido.IdPedido} está cancelado: no se puede entregar.";
                return RedirectToAction(nameof(Index));
            }

            pedido.Estado = "ENTREGADO";
            await _context.SaveChangesAsync();

            // Notificar a la cocina en tiempo real mediante SignalR
            await _hubContext.Clients.Group("Cocina").SendAsync("RecibirCambioEstado", pedido.IdPedido, "ENTREGADO");
            await _hubContext.Clients.Group("Cajero").SendAsync("RecibirCambioEstado", pedido.IdPedido, "ENTREGADO");

            TempData["Exito"] = $"Pedido #{pedido.IdPedido} marcado como ENTREGADO.";
            return RedirectToAction(nameof(Index));
        }
    }
}