using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Hubs;
using ELRINCONDORADO.Models.ApiDtos;
using ELRINCONDORADO.Models;
using ELRINCONDORADO.Helpers;

namespace ELRINCONDORADO.Controllers
{
    public class CajeroController : Controller
    {
        private readonly ProductosApiService _productosApiService;
        private readonly MesasApiService _mesasApiService;
        private readonly VentasApiService _ventasApiService;
        private readonly CierresCajaApiService _cierresCajaApiService;
        private readonly CategoriasApiService _categoriasApiService;
        private readonly PromocionesApiService _promocionesApiService;
        private readonly PedidosApiService _pedidosApiService;
        private readonly ClientesApiService _clientesApiService;
        private readonly IHubContext<PedidosHub> _hub;

        public CajeroController(
            ProductosApiService productosApiService,
            MesasApiService mesasApiService,
            VentasApiService ventasApiService,
            CierresCajaApiService cierresCajaApiService,
            CategoriasApiService categoriasApiService,
            PromocionesApiService promocionesApiService,
            PedidosApiService pedidosApiService,
            ClientesApiService clientesApiService,
            IHubContext<PedidosHub> hub)
        {
            _productosApiService = productosApiService;
            _mesasApiService = mesasApiService;
            _ventasApiService = ventasApiService;
            _cierresCajaApiService = cierresCajaApiService;
            _categoriasApiService = categoriasApiService;
            _promocionesApiService = promocionesApiService;
            _pedidosApiService = pedidosApiService;
            _clientesApiService = clientesApiService;
            _hub = hub;
        }

        private IActionResult? ValidarAcceso()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Rol") != "CAJERO")
                return StatusCode(403, "Solo el cajero puede acceder a esta sección.");

            return null;
        }

        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var productos = await _productosApiService.GetAllAsync(activo: true);
            var mesas = await _mesasApiService.GetAllAsync(estado: "DISPONIBLE");
            var categorias = await _categoriasApiService.GetAllAsync();
            var promociones = await _promocionesApiService.GetAllAsync();

            // Agrupar productos por categoría para la vista
            var productosPorCategoria = productos?
                .GroupBy(p => p.IdCategoria)
                .ToDictionary(g => g.Key, g => g.ToList()) ?? new Dictionary<int, List<ProductoDto>>();

            var categoriasConProductos = categorias?.Select(c => new CategoriaDto
            {
                IdCategoria = c.IdCategoria,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                CantidadProductos = c.CantidadProductos,
                Productos = productosPorCategoria.GetValueOrDefault(c.IdCategoria, new List<ProductoDto>())
            }).ToList() ?? new List<CategoriaDto>();

            var modelo = new CajeroIndexViewModel
            {
                Productos = productos?.ToList() ?? new List<ProductoDto>(),
                Mesas = mesas?.ToList() ?? new List<MesaDto>(),
                Categorias = categoriasConProductos,
                Promociones = promociones?.ToList() ?? new List<PromocionDto>()
            };

            return View("~/Views/Cajero/Index.cshtml", modelo);
        }

        public async Task<IActionResult> FacturaVista(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var venta = await _ventasApiService.GetByIdAsync(id);
            if (venta == null) return NotFound();

            return View("~/Views/Cajero/FacturaVista.cshtml", venta.ToModel());
        }

        // El POS envía el carrito como JSON con el token anti-falsificación en la
        // cabecera RequestVerificationToken. El pedido se crea en la API y luego se
        // avisa a la cocina en tiempo real (hub del MVC, independiente del de la API).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Facturar([FromBody] CrearPedidoRequest request)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var resultado = await _pedidosApiService.CrearAsync(request);
            if (resultado == null || !resultado.Ok)
            {
                var mensaje = resultado?.Mensaje ?? "No se pudo guardar el pedido.";
                return Json(new { ok = false, mensaje });
            }

            await _hub.Clients.Group("Cocina").SendAsync("RecibirNuevoPedido", resultado.IdPedido);

            return Json(new { ok = true, idPedido = resultado.IdPedido });
        }

        // El POS busca el cliente por NIT para completar la razón social del ticket.
        public async Task<IActionResult> BuscarCliente(string? nit)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var resultado = await _clientesApiService.BuscarAsync(nit ?? "");
            if (resultado == null)
                return Json(new { ok = false, mensaje = "No se pudo consultar el servicio de clientes." });

            return Json(resultado);
        }

        public async Task<IActionResult> CierreCajaReporte()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var cajaResumen = await _cierresCajaApiService.GetResumenAsync();

            // Obtener las ventas del día
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var ventasPaginado = await _ventasApiService.GetAllAsync(desde: hoy, hasta: hoy, pageSize: 1000);
            var ventasDto = ventasPaginado?.Items ?? new List<PedidoDto>();
            var ventasModel = ventasDto.Select(v => v.ToModel()).ToList();

            var viewModel = new CierreCajaViewModel
            {
                Resumen = cajaResumen,
                Ventas = ventasModel.Select(v => new CierreCajaVentaDto
                {
                    IdPedido = v.IdPedido,
                    NumeroPedido = v.NumeroPedido,
                    FechaCreacion = v.FechaCreacion,
                    TipoPedido = v.TipoPedido,
                    NombrePedido = v.NombrePedido,
                    EstadoPago = v.EstadoPago,
                    Estado = v.Estado,
                    Total = v.Total,
                    Mesa = v.Mesa != null ? new MesaDto { IdMesa = v.Mesa.IdMesa, Numero = v.Mesa.Numero } : null,
                    Cliente = v.Cliente != null ? new ClienteDto { IdCliente = v.Cliente.IdCliente, RazonSocial = v.Cliente.RazonSocial } : null
                }).ToList(),
                Cantidad = cajaResumen?.VentasHoy ?? 0,
                Subtotal = cajaResumen?.VendidoHoy ?? 0,
                Efectivo = cajaResumen?.PorEstadoPago.FirstOrDefault(p => p.EstadoPago == "EFECTIVO")?.Total ?? 0,
                Tarjeta = cajaResumen?.PorEstadoPago.FirstOrDefault(p => p.EstadoPago == "TARJETA")?.Total ?? 0,
                QR = cajaResumen?.PorEstadoPago.FirstOrDefault(p => p.EstadoPago == "QR")?.Total ?? 0,
                Total = cajaResumen?.VendidoHoy ?? 0,
                Cajero = HttpContext.Session.GetString("Nombre"),
                Fecha = DateTime.Now
            };

            return View("~/Views/Cajero/CierreCajaReporte.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CierreCaja()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            // Crear el cierre de caja en la API
            var empleadoIdStr = HttpContext.Session.GetString("UsuarioId");
            if (!int.TryParse(empleadoIdStr, out var empleadoId))
            {
                TempData["Error"] = "No se pudo identificar al empleado.";
                return RedirectToAction(nameof(Index));
            }

            await _cierresCajaApiService.CrearCierreAsync(empleadoId);

            // Redirigir al reporte de cierre
            return RedirectToAction(nameof(CierreCajaReporte));
        }

        public async Task<IActionResult> CierreCajaPdf()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var cajaResumen = await _cierresCajaApiService.GetResumenAsync();

            // Obtener las ventas del día
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var ventasPaginado = await _ventasApiService.GetAllAsync(desde: hoy, hasta: hoy, pageSize: 1000);
            var ventasDto = ventasPaginado?.Items ?? new List<PedidoDto>();
            var ventasModel = ventasDto.Select(v => v.ToModel()).ToList();

            var viewModel = new CierreCajaViewModel
            {
                Resumen = cajaResumen,
                Ventas = ventasModel.Select(v => new CierreCajaVentaDto
                {
                    IdPedido = v.IdPedido,
                    NumeroPedido = v.NumeroPedido,
                    FechaCreacion = v.FechaCreacion,
                    TipoPedido = v.TipoPedido,
                    NombrePedido = v.NombrePedido,
                    EstadoPago = v.EstadoPago,
                    Estado = v.Estado,
                    Total = v.Total,
                    Mesa = v.Mesa != null ? new MesaDto { IdMesa = v.Mesa.IdMesa, Numero = v.Mesa.Numero } : null,
                    Cliente = v.Cliente != null ? new ClienteDto { IdCliente = v.Cliente.IdCliente, RazonSocial = v.Cliente.RazonSocial } : null
                }).ToList(),
                Cantidad = cajaResumen?.VentasHoy ?? 0,
                Subtotal = cajaResumen?.VendidoHoy ?? 0,
                Efectivo = cajaResumen?.PorEstadoPago.FirstOrDefault(p => p.EstadoPago == "EFECTIVO")?.Total ?? 0,
                Tarjeta = cajaResumen?.PorEstadoPago.FirstOrDefault(p => p.EstadoPago == "TARJETA")?.Total ?? 0,
                QR = cajaResumen?.PorEstadoPago.FirstOrDefault(p => p.EstadoPago == "QR")?.Total ?? 0,
                Total = cajaResumen?.VendidoHoy ?? 0,
                Cajero = HttpContext.Session.GetString("Nombre"),
                Fecha = DateTime.Now
            };

            var pdf = CierreCajaPdfHelper.Generar(viewModel);
            return File(pdf, "application/pdf", $"CierreCaja_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
    }
}
