using Microsoft.AspNetCore.Mvc;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Controllers
{
    public class InsumosController : Controller
    {
        private readonly InsumosApiService _apiService;

        public InsumosController(InsumosApiService apiService)
        {
            _apiService = apiService;
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

            var insumos = await _apiService.GetAllAsync();
            var model = insumos.ToModel();
            return View("~/Views/Administrador/Insumos/Index.cshtml", model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var insumo = await _apiService.GetByIdAsync(id.Value);
            if (insumo == null) return NotFound();

            var model = insumo.ToModel();
            return View("~/Views/Administrador/Insumos/Details.cshtml", model);
        }

        public async Task<IActionResult> Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var destinos = await _apiService.GetDestinosAsync();
            ViewBag.Destinos = destinos;
            return View("~/Views/Administrador/Insumos/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InsumoDto insumo)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (ModelState.IsValid)
            {
                var (success, message) = await _apiService.CreateAsync(insumo);
                if (success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, message);
            }

            var destinos = await _apiService.GetDestinosAsync();
            ViewBag.Destinos = destinos;
            return View("~/Views/Administrador/Insumos/Create.cshtml", insumo);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var insumo = await _apiService.GetByIdAsync(id.Value);
            if (insumo == null) return NotFound();

            var destinos = await _apiService.GetDestinosAsync();
            ViewBag.Destinos = destinos;
            var model = insumo.ToModel();
            return View("~/Views/Administrador/Insumos/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InsumoDto insumo)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id != insumo.IdInsumo) return NotFound();

            if (ModelState.IsValid)
            {
                var (success, message) = await _apiService.EditAsync(id, insumo);
                if (success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, message);
            }

            var destinos = await _apiService.GetDestinosAsync();
            ViewBag.Destinos = destinos;
            return View("~/Views/Administrador/Insumos/Edit.cshtml", insumo);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var insumo = await _apiService.GetByIdAsync(id.Value);
            if (insumo == null) return NotFound();

            var model = insumo.ToModel();
            return View("~/Views/Administrador/Insumos/Delete.cshtml", model);
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
