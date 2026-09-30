using Microsoft.AspNetCore.Mvc;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models.ApiDtos;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class AlmacenController : Controller
    {
        private readonly InsumosApiService _apiService;

        public AlmacenController(InsumosApiService apiService)
        {
            _apiService = apiService;
        }

        private IActionResult? ValidarAcceso()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Rol") != "ALMACEN")
                return StatusCode(403, "Solo el personal de almacén puede acceder a esta sección.");

            return null;
        }

        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var insumosDto = await _apiService.GetAllAsync();
            var insumos = insumosDto?.Select(i => new Insumo
            {
                IdInsumo = i.IdInsumo,
                Nombre = i.Nombre,
                Descripcion = i.Descripcion,
                UnidadMedida = i.UnidadMedida,
                StockActual = i.StockActual,
                StockMinimo = i.StockMinimo,
                CostoUnitario = i.CostoUnitario,
                Activo = i.Activo,
                IdDestino = i.IdDestino,
                DestinoNombre = i.DestinoNombre,
                BajoMinimo = i.BajoMinimo
            }).ToList() ?? new List<Insumo>();

            return View("~/Views/Almacen/Index.cshtml", insumos);
        }
    }
}
