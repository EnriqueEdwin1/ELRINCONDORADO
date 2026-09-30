using Microsoft.AspNetCore.Mvc;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Controllers
{
    public class MovimientosInventarioController : Controller
    {
        private readonly MovimientosInventarioApiService _apiService;

        public MovimientosInventarioController(MovimientosInventarioApiService apiService)
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

            var movimientos = await _apiService.GetAllAsync();
            var model = new MovimientosAdminViewModel
            {
                Movimientos = movimientos.ToModel()
            };
            return View("~/Views/Administrador/MovimientosInventario/Index.cshtml", model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var movimiento = await _apiService.GetByIdAsync(id.Value);
            if (movimiento == null) return NotFound();

            var model = movimiento.ToModel();
            return View("~/Views/Administrador/MovimientosInventario/Details.cshtml", model);
        }

        public IActionResult Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            return View("~/Views/Administrador/MovimientosInventario/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovimientoInventarioDto movimiento)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (ModelState.IsValid)
            {
                var (success, message) = await _apiService.CreateAsync(movimiento);
                if (success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, message);
            }
            var model = movimiento.ToModel();
            return View("~/Views/Administrador/MovimientosInventario/Create.cshtml", model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var movimiento = await _apiService.GetByIdAsync(id.Value);
            if (movimiento == null) return NotFound();

            var model = movimiento.ToModel();
            return View("~/Views/Administrador/MovimientosInventario/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MovimientoInventarioDto movimiento)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id != movimiento.IdMovimiento) return NotFound();

            if (ModelState.IsValid)
            {
                var (success, message) = await _apiService.EditAsync(id, movimiento);
                if (success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, message);
            }
            var model = movimiento.ToModel();
            return View("~/Views/Administrador/MovimientosInventario/Edit.cshtml", model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var movimiento = await _apiService.GetByIdAsync(id.Value);
            if (movimiento == null) return NotFound();

            var model = movimiento.ToModel();
            return View("~/Views/Administrador/MovimientosInventario/Delete.cshtml", model);
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
