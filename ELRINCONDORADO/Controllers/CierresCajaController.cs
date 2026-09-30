using Microsoft.AspNetCore.Mvc;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Controllers
{
    public class CierresCajaController : Controller
    {
        private readonly CierresCajaApiService _apiService;

        public CierresCajaController(CierresCajaApiService apiService)
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

            var cierres = await _apiService.GetAllAsync();
            return View("~/Views/Administrador/CierresCaja/Index.cshtml", cierres);
        }

        public async Task<IActionResult> Details(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var cierre = await _apiService.GetByIdAsync(id);
            if (cierre == null)
            {
                TempData["Error"] = "Cierre de caja no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Administrador/CierresCaja/Details.cshtml", cierre);
        }
    }
}
