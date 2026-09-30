using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly EmpleadosApiService _apiService;
        private readonly RolesApiService _rolesApiService;

        public EmpleadosController(EmpleadosApiService apiService, RolesApiService rolesApiService)
        {
            _apiService = apiService;
            _rolesApiService = rolesApiService;
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

            var empleados = await _apiService.GetAllAsync();
            var model = empleados.ToModel();
            return View("~/Views/Administrador/Empleados/Index.cshtml", model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var empleado = await _apiService.GetByIdAsync(id.Value);
            if (empleado == null) return NotFound();

            var model = empleado.ToModel();
            return View("~/Views/Administrador/Empleados/Details.cshtml", model);
        }

        public async Task<IActionResult> Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var roles = await _rolesApiService.GetAllAsync();
            ViewBag.IdRol = new SelectList(roles, "IdRol", "Nombre");

            return View("~/Views/Administrador/Empleados/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmpleadoDto empleado)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (ModelState.IsValid)
            {
                var (success, message) = await _apiService.CreateAsync(empleado);
                if (success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, message);
            }

            var roles = await _rolesApiService.GetAllAsync();
            ViewBag.IdRol = new SelectList(roles, "IdRol", "Nombre");

            var model = empleado.ToModel();
            return View("~/Views/Administrador/Empleados/Create.cshtml", model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var empleado = await _apiService.GetByIdAsync(id.Value);
            if (empleado == null) return NotFound();

            var roles = await _rolesApiService.GetAllAsync();
            ViewBag.IdRol = new SelectList(roles, "IdRol", "Nombre");

            var model = empleado.ToModel();
            return View("~/Views/Administrador/Empleados/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmpleadoDto empleado)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id != empleado.IdEmpleado) return NotFound();

            if (ModelState.IsValid)
            {
                var (success, message) = await _apiService.EditAsync(id, empleado);
                if (success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, message);
            }

            var roles = await _rolesApiService.GetAllAsync();
            ViewBag.IdRol = new SelectList(roles, "IdRol", "Nombre");

            var model = empleado.ToModel();
            return View("~/Views/Administrador/Empleados/Edit.cshtml", model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var empleado = await _apiService.GetByIdAsync(id.Value);
            if (empleado == null) return NotFound();

            var model = empleado.ToModel();
            return View("~/Views/Administrador/Empleados/Delete.cshtml", model);
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
