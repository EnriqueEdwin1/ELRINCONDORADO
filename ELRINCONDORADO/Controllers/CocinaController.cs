using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class CocinaController : Controller
    {
        private readonly AppDbContext _context;

        public CocinaController(AppDbContext context)
        {
            _context = context;
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

        // GET: Cocina -> cola de pedidos del POS (PENDIENTE y EN PREPARACION) con detalle
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var estados = new List<string> { "PENDIENTE", "EN PREPARACION" };
            var pedidos = await _context.Pedidos
                .Include(p => p.Empleado)
                .Include(p => p.Mesa)
                .Include(p => p.DetallesPedidos)
                    .ThenInclude(d => d.Producto)
                .Where(p => estados.Contains(p.Estado))
                .OrderBy(p => p.Estado == "PENDIENTE" ? 0 : 1) // primero lo pendiente
                    .ThenByDescending(p => p.FechaCreacion)
                .ToListAsync();

            return View(pedidos);
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

            var nuevoEstado = estado?.Trim().ToUpperInvariant();
            if (nuevoEstado != "EN PREPARACION" && nuevoEstado != "LISTO")
            {
                TempData["Error"] = "Estado de cocina inválido.";
                return RedirectToAction(nameof(Index));
            }

            pedido.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            TempData["Exito"] = nuevoEstado == "LISTO"
                ? $"Pedido #{pedido.IdPedido} marcado como LISTO."
                : $"Pedido #{pedido.IdPedido} ahora EN PREPARACION.";
            return RedirectToAction(nameof(Index));
        }
    }
}