using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class DetallePedidsController : Controller
    {
        private readonly AppDbContext _context;

        public DetallePedidsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DetallePedids
        public async Task<IActionResult> Index()
        {
            var detallesPedidos = _context.DetallesPedidos
                .Include(d => d.Pedido)
                .Include(d => d.Producto);
            return View(await detallesPedidos.ToListAsync());
        }

        // GET: DetallePedids/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detallePedido = await _context.DetallesPedidos
                .Include(d => d.Pedido)
                .Include(d => d.Producto)
                .FirstOrDefaultAsync(m => m.IdDetalle == id);
            if (detallePedido == null)
            {
                return NotFound();
            }

            return View(detallePedido);
        }

        // GET: DetallePedids/Create
        public IActionResult Create()
        {
            ViewData["IdPedido"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Pedidos, "IdPedido", "IdPedido");
            ViewData["IdProducto"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Productos, "IdProducto", "Nombre");
            return View();
        }

        // POST: DetallePedids/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDetalle,IdPedido,IdProducto,Cantidad,PrecioUnitario,Subtotal,Observacion")] DetallePedido detallePedido)
        {
            if (ModelState.IsValid)
            {
                _context.Add(detallePedido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdPedido"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Pedidos, "IdPedido", "IdPedido", detallePedido.IdPedido);
            ViewData["IdProducto"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Productos, "IdProducto", "Nombre", detallePedido.IdProducto);
            return View(detallePedido);
        }

        // GET: DetallePedids/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detallePedido = await _context.DetallesPedidos.FindAsync(id);
            if (detallePedido == null)
            {
                return NotFound();
            }
            ViewData["IdPedido"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Pedidos, "IdPedido", "IdPedido", detallePedido.IdPedido);
            ViewData["IdProducto"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Productos, "IdProducto", "Nombre", detallePedido.IdProducto);
            return View(detallePedido);
        }

        // POST: DetallePedids/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDetalle,IdPedido,IdProducto,Cantidad,PrecioUnitario,Subtotal,Observacion")] DetallePedido detallePedido)
        {
            if (id != detallePedido.IdDetalle)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detallePedido);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetallePedidoExists(detallePedido.IdDetalle))
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
            ViewData["IdPedido"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Pedidos, "IdPedido", "IdPedido", detallePedido.IdPedido);
            ViewData["IdProducto"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Productos, "IdProducto", "Nombre", detallePedido.IdProducto);
            return View(detallePedido);
        }

        // GET: DetallePedids/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detallePedido = await _context.DetallesPedidos
                .Include(d => d.Pedido)
                .Include(d => d.Producto)
                .FirstOrDefaultAsync(m => m.IdDetalle == id);
            if (detallePedido == null)
            {
                return NotFound();
            }

            return View(detallePedido);
        }

        // POST: DetallePedids/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detallePedido = await _context.DetallesPedidos.FindAsync(id);
            if (detallePedido != null)
            {
                _context.DetallesPedidos.Remove(detallePedido);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DetallePedidoExists(int id)
        {
            return _context.DetallesPedidos.Any(e => e.IdDetalle == id);
        }
    }
}
