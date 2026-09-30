using Microsoft.AspNetCore.Mvc;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Controllers
{
    public class DestinosInsumosController : Controller
    {
        private readonly InsumosApiService _apiService;

        public DestinosInsumosController(InsumosApiService apiService)
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

            var destinos = await _apiService.GetDestinosAsync();
            var model = destinos.ToModel();
            return View("~/Views/Administrador/DestinosInsumos/Index.cshtml", model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var destinos = await _apiService.GetDestinosAsync();
            var destino = destinos?.FirstOrDefault(d => d.IdDestino == id);
            if (destino == null) return NotFound();

            var model = destino.ToModel();
            return View("~/Views/Administrador/DestinosInsumos/Details.cshtml", model);
        }

        public IActionResult Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            return View("~/Views/Administrador/DestinosInsumos/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DestinoInsumoDto destino)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (ModelState.IsValid)
            {
                var (success, message) = await _apiService.CreateDestinoAsync(destino);
                if (success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, message);
            }
            var model = destino.ToModel();
            return View("~/Views/Administrador/DestinosInsumos/Create.cshtml", model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var destinos = await _apiService.GetDestinosAsync();
            var destino = destinos?.FirstOrDefault(d => d.IdDestino == id);
            if (destino == null) return NotFound();

            var model = destino.ToModel();
            return View("~/Views/Administrador/DestinosInsumos/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DestinoInsumoDto destino)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id != destino.IdDestino) return NotFound();

            if (ModelState.IsValid)
            {
                var (success, message) = await _apiService.EditDestinoAsync(id, destino);
                if (success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, message);
            }
            var model = destino.ToModel();
            return View("~/Views/Administrador/DestinosInsumos/Edit.cshtml", model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var destinos = await _apiService.GetDestinosAsync();
            var destino = destinos?.FirstOrDefault(d => d.IdDestino == id);
            if (destino == null) return NotFound();

            var model = destino.ToModel();
            return View("~/Views/Administrador/DestinosInsumos/Delete.cshtml", model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var (success, message) = await _apiService.DeleteDestinoAsync(id);
            if (!success)
                TempData["Error"] = message;

            return RedirectToAction(nameof(Index));
        }
    }
}
