using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models;
using ELRINCONDORADO.Models.ApiDtos;
using System.Linq;

namespace ELRINCONDORADO.Controllers
{
    public class RecetasController : Controller
    {
        private readonly RecetasApiService _apiService;
        private readonly InsumosApiService _insumosApiService;
        private readonly ProductosApiService _productosApiService;

        public RecetasController(RecetasApiService apiService, InsumosApiService insumosApiService, ProductosApiService productosApiService)
        {
            _apiService = apiService;
            _insumosApiService = insumosApiService;
            _productosApiService = productosApiService;
        }

        private IActionResult? ValidarAcceso()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Rol") != "ADMINISTRADOR")
                return StatusCode(403, "Solo el administrador puede acceder a esta sección.");

            return null;
        }

        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var recetas = await _apiService.GetAllAsync();
            var model = recetas.ToModel();
            return View("~/Views/Administrador/Recetas/Index.cshtml", model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var receta = await _apiService.GetByIdAsync(id.Value);
            if (receta == null) return NotFound();

            var model = receta.ToModel();
            return View("~/Views/Administrador/Recetas/Details.cshtml", model);
        }

        public async Task<IActionResult> Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var insumos = await _insumosApiService.GetAllAsync();
            ViewBag.Insumos = insumos?.Select(i => new Insumo
            {
                IdInsumo = i.IdInsumo,
                Nombre = i.Nombre,
                Descripcion = i.Descripcion,
                UnidadMedida = i.UnidadMedida,
                StockActual = i.StockActual,
                StockMinimo = i.StockMinimo,
                CostoUnitario = i.CostoUnitario,
                Activo = i.Activo,
                IdDestino = i.IdDestino,
                DestinoNombre = i.DestinoNombre,
                BajoMinimo = i.BajoMinimo
            }).ToList() ?? new List<Insumo>();

            // Solo mostrar productos que NO tienen receta
            var productos = await _productosApiService.GetAllAsync();
            var productosSinReceta = productos?.Where(p => !p.TieneReceta).ToList() ?? new List<ProductoDto>();
            ViewBag.IdProducto = new SelectList(productosSinReceta, "IdProducto", "Nombre");

            return View("~/Views/Administrador/Recetas/Create.cshtml", new RecetaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecetaDto receta)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (ModelState.IsValid)
            {
                var (success, message) = await _apiService.CreateAsync(receta);
                if (success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, message);
            }
            var model = receta.ToViewModel();
            return View("~/Views/Administrador/Recetas/Create.cshtml", model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var receta = await _apiService.GetByIdAsync(id.Value);
            if (receta == null) return NotFound();

            var insumos = await _insumosApiService.GetAllAsync();
            ViewBag.Insumos = insumos?.Select(i => new Insumo
            {
                IdInsumo = i.IdInsumo,
                Nombre = i.Nombre,
                Descripcion = i.Descripcion,
                UnidadMedida = i.UnidadMedida,
                StockActual = i.StockActual,
                StockMinimo = i.StockMinimo,
                CostoUnitario = i.CostoUnitario,
                Activo = i.Activo,
                IdDestino = i.IdDestino,
                DestinoNombre = i.DestinoNombre,
                BajoMinimo = i.BajoMinimo
            }).ToList() ?? new List<Insumo>();

            // Pasar el producto actual para mostrarlo como solo lectura
            ViewBag.ProductoActual = receta.ProductoNombre;

            var model = receta.ToViewModel();
            return View("~/Views/Administrador/Recetas/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RecetaDto receta)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id != receta.IdReceta) return NotFound();

            if (ModelState.IsValid)
            {
                var (success, message) = await _apiService.EditAsync(id, receta);
                if (success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, message);
            }
            var model = receta.ToViewModel();
            return View("~/Views/Administrador/Recetas/Edit.cshtml", model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var receta = await _apiService.GetByIdAsync(id.Value);
            if (receta == null) return NotFound();

            var model = receta.ToModel();
            return View("~/Views/Administrador/Recetas/Delete.cshtml", model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var (success, message) = await _apiService.DeleteAsync(id);
            if (!success)
                TempData["Error"] = message;

            return RedirectToAction(nameof(Index));
        }
    }
}
