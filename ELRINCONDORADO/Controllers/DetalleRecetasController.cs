using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class DetalleRecetasController : Controller
    {
        private readonly AppDbContext _context;

        public DetalleRecetasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DetalleRecetas
        public async Task<IActionResult> Index()
        {
            var detalleRecetas = _context.DetalleRecetas
                .Include(d => d.Receta)
                .Include(d => d.Insumo);
            return View(await detalleRecetas.ToListAsync());
        }

        // GET: DetalleRecetas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleReceta = await _context.DetalleRecetas
                .Include(d => d.Receta)
                .Include(d => d.Insumo)
                .FirstOrDefaultAsync(m => m.IdDetalleReceta == id);
            if (detalleReceta == null)
            {
                return NotFound();
            }

            return View(detalleReceta);
        }

        // GET: DetalleRecetas/Create
        public IActionResult Create()
        {
            ViewData["IdReceta"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Recetas, "IdReceta", "IdReceta");
            ViewData["IdInsumo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Insumos, "IdInsumo", "Nombre");
            return View();
        }

        // POST: DetalleRecetas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDetalleReceta,IdReceta,IdInsumo,Cantidad,UnidadMedida")] DetalleReceta detalleReceta)
        {
            if (ModelState.IsValid)
            {
                _context.Add(detalleReceta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdReceta"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Recetas, "IdReceta", "IdReceta", detalleReceta.IdReceta);
            ViewData["IdInsumo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Insumos, "IdInsumo", "Nombre", detalleReceta.IdInsumo);
            return View(detalleReceta);
        }

        // GET: DetalleRecetas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleReceta = await _context.DetalleRecetas.FindAsync(id);
            if (detalleReceta == null)
            {
                return NotFound();
            }
            ViewData["IdReceta"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Recetas, "IdReceta", "IdReceta", detalleReceta.IdReceta);
            ViewData["IdInsumo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Insumos, "IdInsumo", "Nombre", detalleReceta.IdInsumo);
            return View(detalleReceta);
        }

        // POST: DetalleRecetas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDetalleReceta,IdReceta,IdInsumo,Cantidad,UnidadMedida")] DetalleReceta detalleReceta)
        {
            if (id != detalleReceta.IdDetalleReceta)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detalleReceta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetalleRecetaExists(detalleReceta.IdDetalleReceta))
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
            ViewData["IdReceta"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Recetas, "IdReceta", "IdReceta", detalleReceta.IdReceta);
            ViewData["IdInsumo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Insumos, "IdInsumo", "Nombre", detalleReceta.IdInsumo);
            return View(detalleReceta);
        }

        // GET: DetalleRecetas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleReceta = await _context.DetalleRecetas
                .Include(d => d.Receta)
                .Include(d => d.Insumo)
                .FirstOrDefaultAsync(m => m.IdDetalleReceta == id);
            if (detalleReceta == null)
            {
                return NotFound();
            }

            return View(detalleReceta);
        }

        // POST: DetalleRecetas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detalleReceta = await _context.DetalleRecetas.FindAsync(id);
            if (detalleReceta != null)
            {
                _context.DetalleRecetas.Remove(detalleReceta);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DetalleRecetaExists(int id)
        {
            return _context.DetalleRecetas.Any(e => e.IdDetalleReceta == id);
        }
    }
}
