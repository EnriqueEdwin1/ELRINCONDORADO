using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class RecetasController : Controller
    {
        private readonly AppDbContext _context;

        public RecetasController(AppDbContext context)
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

        // GET: Recetas
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var recetas = await _context.Recetas
                .Include(r => r.Producto)
                .Include(r => r.DetalleRecetas)
                .OrderBy(r => r.Producto != null ? r.Producto.Nombre : string.Empty)
                .ToListAsync();

            return View("~/Views/Administrador/Recetas/Index.cshtml", recetas);
        }

        // GET: Recetas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var receta = await _context.Recetas
                .Include(r => r.Producto)
                .Include(r => r.DetalleRecetas)
                    .ThenInclude(d => d.Insumo)
                .FirstOrDefaultAsync(m => m.IdReceta == id);
            if (receta == null)
            {
                return NotFound();
            }

            return View("~/Views/Administrador/Recetas/Details.cshtml", receta);
        }

        // GET: Recetas/Create
        public IActionResult Create(int? idProducto = null)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            CargarListas(idProducto);
            return View("~/Views/Administrador/Recetas/Create.cshtml", new RecetaViewModel { IdProducto = idProducto ?? 0 });
        }

        // POST: Recetas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecetaViewModel modelo)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var detallesValidos = modelo.Detalles
                ?.Where(d => d.IdInsumo > 0 && d.Cantidad > 0)
                .ToList() ?? new List<DetalleRecetaViewModel>();

            if (detallesValidos.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Agrega al menos un insumo con cantidad mayor a cero.");
            }

            if (ModelState.IsValid)
            {
                var receta = new Receta
                {
                    IdProducto = modelo.IdProducto,
                    Descripcion = modelo.Descripcion,
                    Activo = modelo.Activo
                };

                _context.Recetas.Add(receta);
                await _context.SaveChangesAsync();

                foreach (var detalle in detallesValidos)
                {
                    var insumo = await _context.Insumos.FindAsync(detalle.IdInsumo);
                    if (insumo == null)
                        continue;

                    _context.DetalleRecetas.Add(new DetalleReceta
                    {
                        IdReceta = receta.IdReceta,
                        IdInsumo = detalle.IdInsumo,
                        Cantidad = detalle.Cantidad,
                        UnidadMedida = detalle.UnidadMedida ?? insumo.UnidadMedida
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            CargarListas(modelo.IdProducto);
            return View("~/Views/Administrador/Recetas/Create.cshtml", modelo);
        }

        // GET: Recetas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var receta = await _context.Recetas
                .Include(r => r.Producto)
                .Include(r => r.DetalleRecetas)
                    .ThenInclude(d => d.Insumo)
                .FirstOrDefaultAsync(m => m.IdReceta == id);
            if (receta == null)
            {
                return NotFound();
            }

            var modelo = new RecetaViewModel
            {
                IdReceta = receta.IdReceta,
                IdProducto = receta.IdProducto,
                Descripcion = receta.Descripcion,
                Activo = receta.Activo,
                Detalles = receta.DetalleRecetas?
                    .Select(d => new DetalleRecetaViewModel
                    {
                        IdDetalleReceta = d.IdDetalleReceta,
                        IdInsumo = d.IdInsumo,
                        Cantidad = d.Cantidad,
                        UnidadMedida = d.UnidadMedida
                    })
                    .ToList() ?? new List<DetalleRecetaViewModel>()
            };

            CargarListas(modelo.IdProducto);
            return View("~/Views/Administrador/Recetas/Edit.cshtml", modelo);
        }

        // POST: Recetas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RecetaViewModel modelo)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id != modelo.IdReceta)
            {
                return NotFound();
            }

            var detallesValidos = modelo.Detalles
                ?.Where(d => d.IdInsumo > 0 && d.Cantidad > 0)
                .ToList() ?? new List<DetalleRecetaViewModel>();

            if (detallesValidos.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Agrega al menos un insumo con cantidad mayor a cero.");
            }

            if (ModelState.IsValid)
            {
                var receta = await _context.Recetas
                    .Include(r => r.DetalleRecetas)
                    .FirstOrDefaultAsync(r => r.IdReceta == id);
                if (receta == null)
                {
                    return NotFound();
                }

                receta.IdProducto = modelo.IdProducto;
                receta.Descripcion = modelo.Descripcion;
                receta.Activo = modelo.Activo;

                // Reemplazar detalles
                _context.DetalleRecetas.RemoveRange(receta.DetalleRecetas ?? new List<DetalleReceta>());
                foreach (var detalle in detallesValidos)
                {
                    var insumo = await _context.Insumos.FindAsync(detalle.IdInsumo);
                    if (insumo == null)
                        continue;

                    _context.DetalleRecetas.Add(new DetalleReceta
                    {
                        IdReceta = receta.IdReceta,
                        IdInsumo = detalle.IdInsumo,
                        Cantidad = detalle.Cantidad,
                        UnidadMedida = detalle.UnidadMedida ?? insumo.UnidadMedida
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            CargarListas(modelo.IdProducto);
            return View("~/Views/Administrador/Recetas/Edit.cshtml", modelo);
        }

        // GET: Recetas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var receta = await _context.Recetas
                .Include(r => r.Producto)
                .Include(r => r.DetalleRecetas)
                    .ThenInclude(d => d.Insumo)
                .FirstOrDefaultAsync(m => m.IdReceta == id);
            if (receta == null)
            {
                return NotFound();
            }

            return View("~/Views/Administrador/Recetas/Delete.cshtml", receta);
        }

        // POST: Recetas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var receta = await _context.Recetas
                .Include(r => r.DetalleRecetas)
                .FirstOrDefaultAsync(r => r.IdReceta == id);
            if (receta != null)
            {
                _context.DetalleRecetas.RemoveRange(receta.DetalleRecetas ?? new List<DetalleReceta>());
                _context.Recetas.Remove(receta);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool RecetaExists(int id)
        {
            return _context.Recetas.Any(e => e.IdReceta == id);
        }

        private void CargarListas(int? idProducto = null)
        {
            ViewData["IdProducto"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.Productos.Where(p => p.Activo).ToList(), "IdProducto", "Nombre", idProducto);
            ViewBag.Insumos = _context.Insumos.Where(i => i.Activo).OrderBy(i => i.Nombre).ToList();
        }
    }
}
