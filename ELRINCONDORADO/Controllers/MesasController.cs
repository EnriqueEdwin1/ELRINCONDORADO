using Microsoft.AspNetCore.Mvc;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Controllers
{
    public class MesasController : Controller
    {
        private readonly MesasApiService _apiService;

        public MesasController(MesasApiService apiService)
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

            var mesas = await _apiService.GetAllAsync();
            return View("~/Views/Administrador/Mesas/Index.cshtml", mesas);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var mesa = await _apiService.GetByIdAsync(id.Value);
            if (mesa == null) return NotFound();

            return View("~/Views/Administrador/Mesas/Details.cshtml", mesa);
        }

        public IActionResult Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            return View("~/Views/Administrador/Mesas/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MesaDto mesa)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (ModelState.IsValid)
            {
                var (success, message) = await _apiService.CreateAsync(mesa);
                if (success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, message);
            }
            return View("~/Views/Administrador/Mesas/Create.cshtml", mesa);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var mesa = await _apiService.GetByIdAsync(id.Value);
            if (mesa == null) return NotFound();

            return View("~/Views/Administrador/Mesas/Edit.cshtml", mesa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MesaDto mesa)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id != mesa.IdMesa) return NotFound();

            if (ModelState.IsValid)
            {
                var (success, message) = await _apiService.EditAsync(id, mesa);
                if (success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, message);
            }
            return View("~/Views/Administrador/Mesas/Edit.cshtml", mesa);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var mesa = await _apiService.GetByIdAsync(id.Value);
            if (mesa == null) return NotFound();

            return View("~/Views/Administrador/Mesas/Delete.cshtml", mesa);
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
