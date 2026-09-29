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

        // Verifica que haya sesion activa y que el rol sea ADMINISTRADOR.
        // Este controller se genero por scaffolding y estaba SIN ninguna validacion:
        // sin ella, cualquier visita anonima podia crear, editar o BORRAR registros.
        // Sin vistas que lo enlacen, el GET fallaba, pero los POST si se ejecutaban.
        private IActionResult? ValidarAcceso()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Rol") != "ADMINISTRADOR")
                return StatusCode(403, "Solo el administrador puede acceder a esta seccion.");

            return null;
        }

        // GET: DetalleCompras
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var detallesCompras = _context.DetallesCompras
                .Include(d => d.Compra)
                .Include(d => d.Insumo);
            return View(await detallesCompras.ToListAsync());
        }

        // GET: DetalleCompras/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

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
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            ViewData["IdCompra"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Compras, "IdCompra", "IdCompra");
            ViewData["IdInsumo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Insumos, "IdInsumo", "Nombre");
            return View();
        }

        // POST: DetalleCompras/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDetalleCompra,IdCompra,IdInsumo,Cantidad,CostoUnitario,Subtotal")] DetalleCompra detalleCompra)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

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
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

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
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

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
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

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
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

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
