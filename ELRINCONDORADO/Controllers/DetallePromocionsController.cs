using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class DetallePromocionsController : Controller
    {
        private readonly AppDbContext _context;

        public DetallePromocionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DetallePromocions
        public async Task<IActionResult> Index()
        {
            var detallePromociones = _context.DetallePromociones
                .Include(d => d.Promocion)
                .Include(d => d.Producto);
            return View(await detallePromociones.ToListAsync());
        }

        // GET: DetallePromocions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detallePromocion = await _context.DetallePromociones
                .Include(d => d.Promocion)
                .Include(d => d.Producto)
                .FirstOrDefaultAsync(m => m.IdDetallePromocion == id);
            if (detallePromocion == null)
            {
                return NotFound();
            }

            return View(detallePromocion);
        }

        // GET: DetallePromocions/Create
        public IActionResult Create()
        {
            ViewData["IdPromocion"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Promociones, "IdPromocion", "Nombre");
            ViewData["IdProducto"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Productos, "IdProducto", "Nombre");
            return View();
        }

        // POST: DetallePromocions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDetallePromocion,IdPromocion,IdProducto,Cantidad")] DetallePromocion detallePromocion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(detallePromocion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdPromocion"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Promociones, "IdPromocion", "Nombre", detallePromocion.IdPromocion);
            ViewData["IdProducto"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Productos, "IdProducto", "Nombre", detallePromocion.IdProducto);
            return View(detallePromocion);
        }

        // GET: DetallePromocions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detallePromocion = await _context.DetallePromociones.FindAsync(id);
            if (detallePromocion == null)
            {
                return NotFound();
            }
            ViewData["IdPromocion"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Promociones, "IdPromocion", "Nombre", detallePromocion.IdPromocion);
            ViewData["IdProducto"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Productos, "IdProducto", "Nombre", detallePromocion.IdProducto);
            return View(detallePromocion);
        }

        // POST: DetallePromocions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDetallePromocion,IdPromocion,IdProducto,Cantidad")] DetallePromocion detallePromocion)
        {
            if (id != detallePromocion.IdDetallePromocion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detallePromocion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetallePromocionExists(detallePromocion.IdDetallePromocion))
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
            ViewData["IdPromocion"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Promociones, "IdPromocion", "Nombre", detallePromocion.IdPromocion);
            ViewData["IdProducto"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Productos, "IdProducto", "Nombre", detallePromocion.IdProducto);
            return View(detallePromocion);
        }

        // GET: DetallePromocions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detallePromocion = await _context.DetallePromociones
                .Include(d => d.Promocion)
                .Include(d => d.Producto)
                .FirstOrDefaultAsync(m => m.IdDetallePromocion == id);
            if (detallePromocion == null)
            {
                return NotFound();
            }

            return View(detallePromocion);
        }

        // POST: DetallePromocions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detallePromocion = await _context.DetallePromociones.FindAsync(id);
            if (detallePromocion != null)
            {
                _context.DetallePromociones.Remove(detallePromocion);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DetallePromocionExists(int id)
        {
            return _context.DetallePromociones.Any(e => e.IdDetallePromocion == id);
        }
    }
}
