using Microsoft.AspNetCore.Mvc;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Controllers
{
    public class RolesController : Controller
    {
        private readonly RolesApiService _apiService;

        public RolesController(RolesApiService apiService)
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

            var roles = await _apiService.GetAllAsync();
            return View("~/Views/Administrador/Roles/Index.cshtml", roles);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var rol = await _apiService.GetByIdAsync(id.Value);
            if (rol == null) return NotFound();

            return View("~/Views/Administrador/Roles/Details.cshtml", rol);
        }

        public IActionResult Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            return View("~/Views/Administrador/Roles/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RolDto rol)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (ModelState.IsValid)
            {
                var (success, message) = await _apiService.CreateAsync(rol);
                if (success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, message);
            }
            return View("~/Views/Administrador/Roles/Create.cshtml", rol);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var rol = await _apiService.GetByIdAsync(id.Value);
            if (rol == null) return NotFound();

            return View("~/Views/Administrador/Roles/Edit.cshtml", rol);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RolDto rol)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id != rol.IdRol) return NotFound();

            if (ModelState.IsValid)
            {
                var (success, message) = await _apiService.EditAsync(id, rol);
                if (success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, message);
            }
            return View("~/Views/Administrador/Roles/Edit.cshtml", rol);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var rol = await _apiService.GetByIdAsync(id.Value);
            if (rol == null) return NotFound();

            return View("~/Views/Administrador/Roles/Delete.cshtml", rol);
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
