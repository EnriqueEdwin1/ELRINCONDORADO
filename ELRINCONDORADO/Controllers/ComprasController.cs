using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models;
using ELRINCONDORADO.Models.ApiDtos;
using ELRINCONDORADO.Helpers;

namespace ELRINCONDORADO.Controllers
{
    public class ComprasController : Controller
    {
        private readonly ComprasApiService _apiService;
        private readonly ProveedoresApiService _proveedoresApiService;
        private readonly InsumosApiService _insumosApiService;

        public ComprasController(
            ComprasApiService apiService,
            ProveedoresApiService proveedoresApiService,
            InsumosApiService insumosApiService)
        {
            _apiService = apiService;
            _proveedoresApiService = proveedoresApiService;
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

            var resultado = await _apiService.GetAllAsync();
            var compras = (resultado?.Items ?? new List<CompraDto>()).ToModel();
            return View("~/Views/Administrador/Compras/Index.cshtml", compras);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var compra = await _apiService.GetByIdAsync(id.Value);
            if (compra == null) return NotFound();

            var model = compra.ToModel();
            return View("~/Views/Administrador/Compras/Details.cshtml", model);
        }

        public async Task<IActionResult> Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var proveedores = await _proveedoresApiService.GetAllAsync();
            ViewBag.IdProveedor = new SelectList(proveedores, "IdProveedor", "Nombre");

            var insumos = await _insumosApiService.GetAllAsync();
            ViewBag.Insumos = insumos?.Select(i => new Insumo
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

            var destinos = await _insumosApiService.GetDestinosAsync();
            ViewBag.Destinos = destinos?.Select(d => new DestinoInsumo
            {
                IdDestino = d.IdDestino,
                Nombre = d.Nombre
            }).ToList() ?? new List<DestinoInsumo>();

            return View("~/Views/Administrador/Compras/Create.cshtml", new CompraViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int IdProveedor, decimal Descuento, List<CompraItemRequestDto> Detalles)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            // Obtener el ID del empleado de la sesión
            var empleadoIdStr = HttpContext.Session.GetString("UsuarioId");
            if (!int.TryParse(empleadoIdStr, out var empleadoId))
            {
                ModelState.AddModelError(string.Empty, "No se pudo identificar al empleado.");
            }
            else
            {
                // Calcular subtotal y total basado en los detalles recibidos
                var subtotal = 0m;
                if (Detalles != null && Detalles.Any())
                {
                    foreach (var detalle in Detalles)
                    {
                        subtotal += detalle.Cantidad * detalle.CostoUnitario;
                        detalle.Subtotal = detalle.Cantidad * detalle.CostoUnitario;
                    }
                }
                var total = Math.Max(subtotal - Descuento, 0);

                var compra = new CompraConDetallesDto
                {
                    IdProveedor = IdProveedor,
                    IdEmpleado = empleadoId,
                    FechaCompra = DateTime.Now,
                    Subtotal = subtotal,
                    Descuento = Descuento,
                    Total = total,
                    Estado = "REGISTRADA",
                    Detalles = Detalles?.Select(d => new CompraDetalleRequestDto
                    {
                        IdInsumo = d.IdInsumo,
                        Cantidad = d.Cantidad,
                        CostoUnitario = d.CostoUnitario,
                        Subtotal = d.Subtotal
                    }).ToList() ?? new List<CompraDetalleRequestDto>()
                };

                var (success, message) = await _apiService.CreateConDetallesAsync(compra);
                if (success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, message);
            }

            var proveedores = await _proveedoresApiService.GetAllAsync();
            ViewBag.IdProveedor = new SelectList(proveedores, "IdProveedor", "Nombre");

            var insumos = await _insumosApiService.GetAllAsync();
            ViewBag.Insumos = insumos?.Select(i => new Insumo
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

            var destinos = await _insumosApiService.GetDestinosAsync();
            ViewBag.Destinos = destinos?.Select(d => new DestinoInsumo
            {
                IdDestino = d.IdDestino,
                Nombre = d.Nombre
            }).ToList() ?? new List<DestinoInsumo>();

            return View("~/Views/Administrador/Compras/Create.cshtml", new CompraViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CrearInsumoJson(
            string nombre, string? descripcion, string unidadMedida,
            int idDestino, decimal costoUnitario, decimal? stockMinimo,
            decimal? stockActual, bool activo)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return Json(new { ok = false, error = "No autorizado" });

            var insumo = new InsumoDto
            {
                Nombre = nombre,
                Descripcion = descripcion,
                UnidadMedida = unidadMedida,
                IdDestino = idDestino,
                CostoUnitario = costoUnitario,
                StockMinimo = stockMinimo ?? 0,
                StockActual = stockActual ?? 0,
                Activo = activo
            };

            var (success, message) = await _insumosApiService.CreateAsync(insumo);
            if (!success)
                return Json(new { ok = false, error = message });

            // Recargar para obtener el ID generado
            var insumos = await _insumosApiService.GetAllAsync();
            var creado = insumos?.FirstOrDefault(i => i.Nombre == nombre);
            if (creado == null)
                return Json(new { ok = false, error = "Insumo creado pero no se pudo encontrar en la lista" });

            return Json(new
            {
                ok = true,
                insumo = new
                {
                    IdInsumo = creado.IdInsumo,
                    Nombre = creado.Nombre,
                    UnidadMedida = creado.UnidadMedida,
                    CostoUnitario = creado.CostoUnitario
                }
            });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var compra = await _apiService.GetByIdAsync(id.Value);
            if (compra == null) return NotFound();

            var model = compra.ToModel();
            return View("~/Views/Administrador/Compras/Delete.cshtml", model);
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

        public async Task<IActionResult> Anular(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var compra = await _apiService.GetByIdAsync(id.Value);
            if (compra == null) return NotFound();

            var model = compra.ToModel();
            return View("~/Views/Administrador/Compras/Anular.cshtml", model);
        }

        [HttpPost, ActionName("Anular")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnularConfirmed(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var (success, message) = await _apiService.AnularAsync(id);
            if (!success)
                TempData["Error"] = message;
            else
                TempData["Mensaje"] = "Compra anulada exitosamente. El stock de insumos ha sido descontado.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Comprobante(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var compra = await _apiService.GetByIdAsync(id);
            if (compra == null) return NotFound();

            var model = compra.ToModel();
            return View("~/Views/Administrador/Compras/Comprobante.cshtml", model);
        }

        public async Task<IActionResult> Reporte(string tipo, string fecha)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            DateTime inicio, fin;

            if (tipo == "dia")
            {
                if (!DateTime.TryParse(fecha, out inicio))
                {
                    inicio = DateTime.Today;
                }
                fin = inicio.AddDays(1).AddTicks(-1);
            }
            else if (tipo == "mes")
            {
                if (!DateTime.TryParse(fecha, out inicio))
                {
                    inicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                }
                fin = inicio.AddMonths(1).AddDays(-1);
            }
            else
            {
                inicio = DateTime.Today;
                fin = DateTime.Today.AddDays(1).AddTicks(-1);
            }

            // Obtener todas las compras del periodo sin paginación
            var resultado = await _apiService.GetAllAsync(
                desde: DateOnly.FromDateTime(inicio),
                hasta: DateOnly.FromDateTime(fin),
                pageSize: 1000 // Aumentar pageSize para obtener todas las compras
            );

            var compras = (resultado?.Items ?? new List<CompraDto>()).ToModel();

            // Excluir compras anuladas
            compras = compras.Where(c => c.Estado != "ANULADO").ToList();

            var titulo = tipo == "dia" ? $"Reporte de compras del {inicio:dd/MM/yyyy}"
                         : $"Reporte de compras de {inicio:MMMM yyyy}";

            var pdf = ReporteComprasPdfHelper.Generar(compras, inicio, fin, titulo);
            return File(pdf, "application/pdf", $"ReporteCompras_{inicio:yyyyMMdd}.pdf");
        }
    }
}
