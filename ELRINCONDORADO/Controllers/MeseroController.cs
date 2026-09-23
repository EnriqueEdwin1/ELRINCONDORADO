using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class MeseroController : Controller
    {
        private readonly AppDbContext _context;

        public MeseroController(AppDbContext context)
        {
            _context = context;
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

            pedido.Estado = "ENTREGADO";
            await _context.SaveChangesAsync();

            TempData["Exito"] = $"Pedido #{pedido.IdPedido} marcado como ENTREGADO.";
            return RedirectToAction(nameof(Index));
        }
    }
}