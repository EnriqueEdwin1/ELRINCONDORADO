using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class MovimientosInventarioController : Controller
    {
        private readonly AppDbContext _context;

        public MovimientosInventarioController(AppDbContext context)
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

        // GET: MovimientosInventario
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var movimientos = _context.MovimientosInventario
                .Include(m => m.Insumo)
                .Include(m => m.Empleado)
                .OrderByDescending(m => m.Fecha);
            return View("~/Views/Administrador/MovimientosInventario/Index.cshtml", await movimientos.ToListAsync());
        }

        // GET: MovimientosInventario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var movimientoInventario = await _context.MovimientosInventario
                .Include(m => m.Insumo)
                .Include(m => m.Empleado)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);
            if (movimientoInventario == null)
            {
                return NotFound();
            }

            return View("~/Views/Administrador/MovimientosInventario/Details.cshtml", movimientoInventario);
        }

        // GET: MovimientosInventario/Create
        public async Task<IActionResult> Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            await CargarListadosAsync(null,
                new MovimientoInventario { Fecha = DateTime.Now });
            return View("~/Views/Administrador/MovimientosInventario/Create.cshtml",
                new MovimientoInventario { Fecha = DateTime.Now });
        }

        // POST: MovimientosInventario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdMovimiento,IdInsumo,IdEmpleado,TipoMovimiento,Cantidad,Fecha,Motivo")] MovimientoInventario movimientoInventario)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            await CargarListadosAsync(movimientoInventario, movimientoInventario);

            if (ModelState.IsValid)
            {
                if (movimientoInventario.Cantidad <= 0)
                {
                    ModelState.AddModelError(string.Empty, "La cantidad debe ser mayor a 0.");
                    return View("~/Views/Administrador/MovimientosInventario/Create.cshtml", movimientoInventario);
                }

                _context.Add(movimientoInventario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Administrador/MovimientosInventario/Create.cshtml", movimientoInventario);
        }

        // GET: MovimientosInventario/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var movimientoInventario = await _context.MovimientosInventario.FindAsync(id);
            if (movimientoInventario == null)
            {
                return NotFound();
            }

            await CargarListadosAsync(movimientoInventario, movimientoInventario);
            return View("~/Views/Administrador/MovimientosInventario/Edit.cshtml", movimientoInventario);
        }

        // POST: MovimientosInventario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdMovimiento,IdInsumo,IdEmpleado,TipoMovimiento,Cantidad,Fecha,Motivo")] MovimientoInventario movimientoInventario)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id != movimientoInventario.IdMovimiento)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movimientoInventario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovimientoInventarioExists(movimientoInventario.IdMovimiento))
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
            await CargarListadosAsync(movimientoInventario, movimientoInventario);
            return View("~/Views/Administrador/MovimientosInventario/Edit.cshtml", movimientoInventario);
        }

        // GET: MovimientosInventario/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var movimientoInventario = await _context.MovimientosInventario
                .Include(m => m.Insumo)
                .Include(m => m.Empleado)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);
            if (movimientoInventario == null)
            {
                return NotFound();
            }

            return View("~/Views/Administrador/MovimientosInventario/Delete.cshtml", movimientoInventario);
        }

        // POST: MovimientosInventario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var movimientoInventario = await _context.MovimientosInventario.FindAsync(id);
            if (movimientoInventario != null)
            {
                _context.MovimientosInventario.Remove(movimientoInventario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MovimientoInventarioExists(int id)
        {
            return _context.MovimientosInventario.Any(e => e.IdMovimiento == id);
        }

        // Carga los listados de insumos y empleados para los selects de los formularios
        private async Task CargarListadosAsync(MovimientoInventario? seleccionado, MovimientoInventario modelo)
        {
            ViewData["IdInsumo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Insumos.OrderBy(i => i.Nombre).ToListAsync(),
                "IdInsumo", "Nombre", seleccionado?.IdInsumo);
            ViewData["IdEmpleado"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Empleados.OrderBy(e => e.Usuario).ToListAsync(),
                "IdEmpleado", "Usuario", seleccionado?.IdEmpleado);
        }
    }
}
