using Microsoft.AspNetCore.Mvc;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Controllers
{
    public class DetalleRecetasController : Controller
    {
        private readonly RecetasApiService _apiService;

        public DetalleRecetasController(RecetasApiService apiService)
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

            var recetas = await _apiService.GetAllAsync();
            return View("~/Views/Administrador/Recetas/Index.cshtml", recetas);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var receta = await _apiService.GetByIdAsync(id.Value);
            if (receta == null) return NotFound();

            return View("~/Views/Administrador/Recetas/Details.cshtml", receta);
        }
    }
}
