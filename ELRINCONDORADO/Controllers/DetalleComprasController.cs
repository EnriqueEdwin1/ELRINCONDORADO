using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class DetalleComprasController : Controller
    {
        private readonly AppDbContext _context;

        public DetalleComprasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DetalleCompras
        public async Task<IActionResult> Index()
        {
            var detallesCompras = _context.DetallesCompras
                .Include(d => d.Compra)
                .Include(d => d.Insumo);
            return View(await detallesCompras.ToListAsync());
        }

        // GET: DetalleCompras/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleCompra = await _context.DetallesCompras
                .Include(d => d.Compra)
                .Include(d => d.Insumo)
                .FirstOrDefaultAsync(m => m.IdDetalleCompra == id);
            if (detalleCompra == null)
            {
                return NotFound();
            }

            return View(detalleCompra);
        }

        // GET: DetalleCompras/Create
        public IActionResult Create()
        {
            ViewData["IdCompra"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Compras, "IdCompra", "IdCompra");
            ViewData["IdInsumo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Insumos, "IdInsumo", "Nombre");
            return View();
        }

        // POST: DetalleCompras/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDetalleCompra,IdCompra,IdInsumo,Cantidad,CostoUnitario,Subtotal")] DetalleCompra detalleCompra)
        {
            if (ModelState.IsValid)
            {
                _context.Add(detalleCompra);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCompra"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Compras, "IdCompra", "IdCompra", detalleCompra.IdCompra);
            ViewData["IdInsumo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Insumos, "IdInsumo", "Nombre", detalleCompra.IdInsumo);
            return View(detalleCompra);
        }

        // GET: DetalleCompras/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleCompra = await _context.DetallesCompras.FindAsync(id);
            if (detalleCompra == null)
            {
                return NotFound();
            }
            ViewData["IdCompra"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Compras, "IdCompra", "IdCompra", detalleCompra.IdCompra);
            ViewData["IdInsumo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Insumos, "IdInsumo", "Nombre", detalleCompra.IdInsumo);
            return View(detalleCompra);
        }

        // POST: DetalleCompras/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDetalleCompra,IdCompra,IdInsumo,Cantidad,CostoUnitario,Subtotal")] DetalleCompra detalleCompra)
        {
            if (id != detalleCompra.IdDetalleCompra)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detalleCompra);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetalleCompraExists(detalleCompra.IdDetalleCompra))
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
            ViewData["IdCompra"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Compras, "IdCompra", "IdCompra", detalleCompra.IdCompra);
            ViewData["IdInsumo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Insumos, "IdInsumo", "Nombre", detalleCompra.IdInsumo);
            return View(detalleCompra);
        }

        // GET: DetalleCompras/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleCompra = await _context.DetallesCompras
                .Include(d => d.Compra)
                .Include(d => d.Insumo)
                .FirstOrDefaultAsync(m => m.IdDetalleCompra == id);
            if (detalleCompra == null)
            {
                return NotFound();
            }

            return View(detalleCompra);
        }

        // POST: DetalleCompras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detalleCompra = await _context.DetallesCompras.FindAsync(id);
            if (detalleCompra != null)
            {
                _context.DetallesCompras.Remove(detalleCompra);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DetalleCompraExists(int id)
        {
            return _context.DetallesCompras.Any(e => e.IdDetalleCompra == id);
        }
    }
}
