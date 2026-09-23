using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class DestinosInsumosController : Controller
    {
        private readonly AppDbContext _context;

        public DestinosInsumosController(AppDbContext context)
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

        // GET: DestinosInsumos
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            return View("~/Views/Administrador/DestinosInsumos/Index.cshtml",
                await _context.DestinosInsumos.Include(d => d.Insumos).ToListAsync());
        }

        // GET: DestinosInsumos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var destino = await _context.DestinosInsumos
                .Include(d => d.Insumos)
                .FirstOrDefaultAsync(m => m.IdDestino == id);
            if (destino == null)
            {
                return NotFound();
            }

            return View("~/Views/Administrador/DestinosInsumos/Details.cshtml", destino);
        }

        // GET: DestinosInsumos/Create
        public IActionResult Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            return View("~/Views/Administrador/DestinosInsumos/Create.cshtml", new DestinoInsumo());
        }

        // POST: DestinosInsumos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDestino,Nombre")] DestinoInsumo destino)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (ModelState.IsValid)
            {
                if (await _context.DestinosInsumos.AnyAsync(d => d.Nombre == destino.Nombre))
                {
                    ModelState.AddModelError(string.Empty, "Ese destino ya existe.");
                    return View("~/Views/Administrador/DestinosInsumos/Create.cshtml", destino);
                }

                _context.Add(destino);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Administrador/DestinosInsumos/Create.cshtml", destino);
        }

        // GET: DestinosInsumos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var destino = await _context.DestinosInsumos.FindAsync(id);
            if (destino == null)
            {
                return NotFound();
            }
            return View("~/Views/Administrador/DestinosInsumos/Edit.cshtml", destino);
        }

        // POST: DestinosInsumos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDestino,Nombre")] DestinoInsumo destino)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id != destino.IdDestino)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await _context.DestinosInsumos.AnyAsync(d => d.Nombre == destino.Nombre && d.IdDestino != destino.IdDestino))
                {
                    ModelState.AddModelError(string.Empty, "Ese destino ya existe.");
                    return View("~/Views/Administrador/DestinosInsumos/Edit.cshtml", destino);
                }

                try
                {
                    _context.Update(destino);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DestinoInsumoExists(destino.IdDestino))
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
            return View("~/Views/Administrador/DestinosInsumos/Edit.cshtml", destino);
        }

        // GET: DestinosInsumos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var destino = await _context.DestinosInsumos
                .FirstOrDefaultAsync(m => m.IdDestino == id);
            if (destino == null)
            {
                return NotFound();
            }

            return View("~/Views/Administrador/DestinosInsumos/Delete.cshtml", destino);
        }

        // POST: DestinosInsumos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var destino = await _context.DestinosInsumos.FindAsync(id);
            if (destino != null)
            {
                if (await _context.Insumos.AnyAsync(i => i.IdDestino == id))
                {
                    TempData["Error"] = "No se puede eliminar el destino porque hay insumos asignados a él.";
                    return RedirectToAction(nameof(Index));
                }

                _context.DestinosInsumos.Remove(destino);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DestinoInsumoExists(int id)
        {
            return _context.DestinosInsumos.Any(e => e.IdDestino == id);
        }
    }
}