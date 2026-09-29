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

        // GET: DetallePromocions
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var detallePromociones = _context.DetallePromociones
                .Include(d => d.Promocion)
                .Include(d => d.Producto);
            return View(await detallePromociones.ToListAsync());
        }

        // GET: DetallePromocions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

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
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            ViewData["IdPromocion"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Promociones, "IdPromocion", "Nombre");
            ViewData["IdProducto"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Productos, "IdProducto", "Nombre");
            return View();
        }

        // POST: DetallePromocions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDetallePromocion,IdPromocion,IdProducto,Cantidad")] DetallePromocion detallePromocion)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

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
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

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
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

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
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

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
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

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
