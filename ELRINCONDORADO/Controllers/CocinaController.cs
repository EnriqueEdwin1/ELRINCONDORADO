using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;
using ELRINCONDORADO.Services;

namespace ELRINCONDORADO.Controllers
{
    public class CocinaController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PantallaCocinaTracker _pantallasCocina;

        public CocinaController(AppDbContext context, PantallaCocinaTracker pantallasCocina)
        {
            _context = context;
            _pantallasCocina = pantallasCocina;
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

        // GET: Cocina/ActualizarCola -> JSON de la cola activa para la pantalla en vivo
        // (la vista lo consulta en intervalos: anuncia por voz los pedidos PENDIENTE nuevos)
        [HttpGet]
        public async Task<IActionResult> ActualizarCola()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var estados = new List<string> { "PENDIENTE", "EN PREPARACION" };
            var pedidos = await _context.Pedidos
                .Where(p => estados.Contains(p.Estado))
                .OrderBy(p => p.Estado == "PENDIENTE" ? 0 : 1) // primero lo pendiente
                    .ThenByDescending(p => p.FechaCreacion)
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

            return Json(pedidos);
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