using Microsoft.AspNetCore.Mvc;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Controllers
{
    public class DetallePedidsController : Controller
    {
        private readonly VentasApiService _apiService;

        public DetallePedidsController(VentasApiService apiService)
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

            var ventas = await _apiService.GetAllAsync();
            return View("~/Views/Administrador/Ventas/Index.cshtml", ventas);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var venta = await _apiService.GetByIdAsync(id.Value);
            if (venta == null) return NotFound();

            return View("~/Views/Administrador/Ventas/Details.cshtml", venta);
        }
    }
}
