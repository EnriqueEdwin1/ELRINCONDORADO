using Microsoft.AspNetCore.Mvc;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Controllers
{
    public class DetalleComprasController : Controller
    {
        private readonly ComprasApiService _apiService;

        public DetalleComprasController(ComprasApiService apiService)
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

            var compras = await _apiService.GetAllAsync();
            return View("~/Views/Administrador/Compras/Index.cshtml", compras);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var compra = await _apiService.GetByIdAsync(id.Value);
            if (compra == null) return NotFound();

            return View("~/Views/Administrador/Compras/Details.cshtml", compra);
        }
    }
}
