using Microsoft.AspNetCore.Mvc;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models.ApiDtos;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class MeseroController : Controller
    {
        private readonly MeseroApiService _apiService;

        public MeseroController(MeseroApiService apiService)
        {
            _apiService = apiService;
        }

        private IActionResult? ValidarAcceso()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Rol") != "MESERO")
                return StatusCode(403, "Solo el mesero puede acceder a esta sección.");

            return null;
        }

        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var pedidosDto = await _apiService.GetPedidosEntregarAsync();
            var pedidos = pedidosDto?.Select(p => p.ToModel()).ToList() ?? new List<Pedido>();

            return View("~/Views/Mesero/Index.cshtml", pedidos);
        }

        public async Task<IActionResult> Historial()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var empleadoIdStr = HttpContext.Session.GetString("UsuarioId");
            if (!int.TryParse(empleadoIdStr, out var empleadoId))
            {
                TempData["Error"] = "No se pudo identificar al mesero.";
                return RedirectToAction(nameof(Index));
            }

            var pedidosDto = await _apiService.GetHistorialAsync(empleadoId);
            var pedidos = pedidosDto?.Select(p => p.ToModel()).ToList() ?? new List<Pedido>();

            return View("~/Views/Mesero/Historial.cshtml", pedidos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entregar(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var (success, message) = await _apiService.EntregarAsync(id);
            if (!success)
            {
                TempData["Error"] = message;
            }
            else
            {
                TempData["Exito"] = "Pedido entregado exitosamente.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
