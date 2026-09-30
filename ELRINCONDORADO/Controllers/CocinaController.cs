using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Services;
using ELRINCONDORADO.Hubs;
using ELRINCONDORADO.Models.ApiDtos;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class CocinaController : Controller
    {
        private readonly CocinaApiService _apiService;
        private readonly PedidosApiService _pedidosApiService;
        private readonly IHubContext<PedidosHub> _hub;
        private readonly PantallaCocinaTracker _pantallasCocina;

        public CocinaController(
            CocinaApiService apiService,
            PedidosApiService pedidosApiService,
            IHubContext<PedidosHub> hub,
            PantallaCocinaTracker pantallasCocina)
        {
            _apiService = apiService;
            _pedidosApiService = pedidosApiService;
            _hub = hub;
            _pantallasCocina = pantallasCocina;
        }

        private IActionResult? ValidarAcceso()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Rol") != "COCINERO")
                return StatusCode(403, "Solo el cocinero puede acceder a esta sección.");

            return null;
        }

        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var cola = await _apiService.GetColaAsync();
            var pedidos = cola?.Select(p => p.ToModel()).ToList() ?? new List<Pedido>();

            return View("~/Views/Cocina/Index.cshtml", pedidos);
        }

        // Refresca la cola de la pantalla de cocina. Devuelve el array JSON
        // directamente (el JS hace JSON.parse y lo pinta con renderCola).
        public async Task<IActionResult> ActualizarCola()
        {
            var cola = await _apiService.GetColaAsync();
            return Json(cola ?? new List<ColaPedidoDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string estado)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var ok = await _pedidosApiService.CambiarEstadoAsync(id, estado);
            if (ok)
            {
                // Avisar a las pantallas en tiempo real (sin tocar la API: el hub
                // del MVC es independiente del de la API).
                await _hub.Clients.Group("Cocina").SendAsync("RecibirCambioEstado", id, estado);
                await _hub.Clients.Group("Cajero").SendAsync("RecibirCambioEstado", id, estado);
                if (estado == "LISTO")
                    await _hub.Clients.Group("Mesero").SendAsync("RecibirCambioEstado", id, estado);
            }

            return RedirectToAction("Index");
        }

        // Latido de la pantalla de cocina: registra que hay alguien mirando la cola.
        // Sin el acceso porque es un fetch que se repite cada 10 s desde el navegador.
        public IActionResult Ping()
        {
            _pantallasCocina.Registrar(HttpContext.Session.Id);
            return Ok();
        }
    }
}