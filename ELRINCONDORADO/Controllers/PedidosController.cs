using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class PedidosController : Controller
    {
        private readonly AppDbContext _context;

        public PedidosController(AppDbContext context)
        {
            _context = context;
        }

        // Verifica que haya sesión activa y que el rol sea ADMINISTRADOR
        private IActionResult? ValidarAcceso()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Rol") != "ADMINISTRADOR")
                return StatusCode(403, "Solo el administrador puede acceder a esta sección.");

            return null;
        }

        // GET: Pedidos
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var pedidos = _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Empleado)
                .Include(p => p.Promocion)
                .Include(p => p.Cliente)
                .OrderByDescending(p => p.FechaCreacion);
            return View("~/Views/Administrador/Pedidos/Index.cshtml", await pedidos.ToListAsync());
        }

        // GET: Pedidos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Empleado)
                .Include(p => p.Promocion)
                .Include(p => p.DetallesPedidos)
                .FirstOrDefaultAsync(m => m.IdPedido == id);
            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        // GET: Pedidos/Create
        public IActionResult Create()
        {
            ViewData["IdMesa"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Mesas, "IdMesa", "Numero");
            ViewData["IdEmpleado"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Empleados, "IdEmpleado", "Usuario");
            ViewData["IdPromocion"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Promociones, "IdPromocion", "Nombre");
            return View();
        }

        // POST: Pedidos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPedido,IdMesa,IdEmpleado,IdPromocion,TipoPedido,Estado,EstadoPago,FechaCreacion,Subtotal,Descuento,Total,Observaciones")] Pedido pedido)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pedido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdMesa"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Mesas, "IdMesa", "Numero", pedido.IdMesa);
            ViewData["IdEmpleado"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Empleados, "IdEmpleado", "Usuario", pedido.IdEmpleado);
            ViewData["IdPromocion"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Promociones, "IdPromocion", "Nombre", pedido.IdPromocion);
            return View(pedido);
        }

        // GET: Pedidos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }
            ViewData["IdMesa"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Mesas, "IdMesa", "Numero", pedido.IdMesa);
            ViewData["IdEmpleado"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Empleados, "IdEmpleado", "Usuario", pedido.IdEmpleado);
            ViewData["IdPromocion"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Promociones, "IdPromocion", "Nombre", pedido.IdPromocion);
            return View(pedido);
        }

        // POST: Pedidos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPedido,IdMesa,IdEmpleado,IdPromocion,TipoPedido,Estado,EstadoPago,FechaCreacion,Subtotal,Descuento,Total,Observaciones")] Pedido pedido)
        {
            if (id != pedido.IdPedido)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pedido);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PedidoExists(pedido.IdPedido))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdMesa"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Mesas, "IdMesa", "Numero", pedido.IdMesa);
            ViewData["IdEmpleado"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Empleados, "IdEmpleado", "Usuario", pedido.IdEmpleado);
            ViewData["IdPromocion"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Promociones, "IdPromocion", "Nombre", pedido.IdPromocion);
            return View(pedido);
        }

        // GET: Pedidos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Empleado)
                .Include(p => p.Promocion)
                .FirstOrDefaultAsync(m => m.IdPedido == id);
            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        // POST: Pedidos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido != null)
            {
                _context.Pedidos.Remove(pedido);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PedidoExists(int id)
        {
            return _context.Pedidos.Any(e => e.IdPedido == id);
        }
    }
}
