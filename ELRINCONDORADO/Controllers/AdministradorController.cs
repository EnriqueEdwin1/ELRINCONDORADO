using Microsoft.AspNetCore.Mvc;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models.ApiDtos;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class AdministradorController : Controller
    {
        private readonly CierresCajaApiService _cierresCajaApiService;
        private readonly InsumosApiService _insumosApiService;

        public AdministradorController(
            CierresCajaApiService cierresCajaApiService,
            InsumosApiService insumosApiService)
        {
            _cierresCajaApiService = cierresCajaApiService;
            _insumosApiService = insumosApiService;
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

            var cajaResumen = await _cierresCajaApiService.GetResumenAsync();
            var insumosBajoMinimo = (await _insumosApiService.GetAllAsync(bajoMinimo: true))?.ToList();

            var modelo = new AdminResumenViewModel
            {
                CajaResumen = cajaResumen,
                InsumosBajoMinimo = insumosBajoMinimo ?? new List<InsumoDto>(),
                UltimoCierreLocal = cajaResumen?.UltimoCierreFecha,
                VendidoTurno = cajaResumen?.VendidoTurno ?? 0,
                VentasTurno = cajaResumen?.VentasTurno ?? 0,
                VendidoHoy = cajaResumen?.VendidoHoy ?? 0,
                VentasHoy = cajaResumen?.VentasHoy ?? 0
            };

            return View("~/Views/Administrador/Index.cshtml", modelo);
        }
    }
}
