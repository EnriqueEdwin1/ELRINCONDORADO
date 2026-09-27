using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Helpers;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    /// <summary>
    /// Pantalla de Ventas del administrador: lista todos los pedidos facturados, permite ver
    /// el detalle y reimprimir la factura. Los datos salen solo de pedidos + detalle_pedidos
    /// (la tabla ventas se eliminó porque duplicaba esa información).
    /// </summary>
    public class VentasController : Controller
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context)
        {
            _context = context;
        }

        // Verifica que haya sesión activa y que el rol sea ADMINISTRADOR
        private IActionResult? ValidarAcceso()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Rol") != "ADMINISTRADOR")
                return StatusCode(403, "Solo el administrador puede acceder a esta sección.");

            return null;
        }

        // GET: Ventas
        public async Task<IActionResult> Index(string? desde, string? hasta)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var (fechaDesde, fechaHasta, periodo) = ResolverRango(desde, hasta);

            // Solo ventas facturadas: el cajero siempre registra el método de pago al facturar,
            // así que un pedido sin método (o pendiente) es una comanda de cocina, no una venta.
            var consulta = _context.Pedidos
                .AsNoTracking()
                .Where(p => p.EstadoPago != null && p.EstadoPago != "" && p.EstadoPago != "PENDIENTE")
                .Include(p => p.Mesa)
                .Include(p => p.Empleado)
                .Include(p => p.Cliente)
                .AsQueryable();

            if (fechaDesde.HasValue)
            {
                consulta = consulta.Where(p => p.FechaCreacion >= fechaDesde.Value);

                if (fechaHasta.HasValue)
                {
                    // Se compara contra el día siguiente para tomar el día "Hasta" completo.
                    var fin = fechaHasta.Value.Date.AddDays(1);
                    consulta = consulta.Where(p => p.FechaCreacion < fin);
                }
            }

            var modelo = new VentasListaViewModel
            {
                Pedidos = await consulta
                    .OrderByDescending(p => p.FechaCreacion)
                    .ThenByDescending(p => p.IdPedido)
                    .ToListAsync(),
                Desde = fechaDesde,
                Hasta = fechaHasta,
                Periodo = periodo
            };

            return View("~/Views/Administrador/Ventas/Index.cshtml", modelo);
        }

        // GET: Ventas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var pedido = await CargarPedido(id.Value);
            if (pedido == null) return NotFound();

            return View("~/Views/Administrador/Ventas/Details.cshtml", pedido);
        }

        // GET: Ventas/FacturaVista/5 -> factura en el navegador, lista para reimprimir
        public async Task<IActionResult> FacturaVista(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var pedido = await CargarPedido(id.Value);
            if (pedido == null) return NotFound();

            // La vista de factura es la misma del cajero, pero parametrizada para que el botón
            // de volver y el PDF apunten a esta sección en vez de al POS.
            ViewData["VolverUrl"] = "/Ventas";
            ViewData["VolverTexto"] = "← Ventas";
            ViewData["PdfUrl"] = $"/Ventas/Factura/{pedido.IdPedido}";

            return View("~/Views/Cajero/FacturaVista.cshtml", pedido);
        }

        // GET: Ventas/Factura/5 -> factura en PDF (reimpresión)
        public async Task<IActionResult> Factura(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var pedido = await CargarPedido(id.Value);
            if (pedido == null) return NotFound();

            var pdf = FacturaPdfHelper.Generar(pedido);
            return File(pdf, "application/pdf", $"Factura_Pedido_{pedido.IdPedido:D4}.pdf");
        }

        // POST: Ventas/Cancelar/5 -> el administrador anula un pedido facturado.
        // El pedido queda CANCELADO (deja de sumar en el cierre de caja) y los insumos de las
        // recetas vuelven al inventario como movimientos de tipo INGRESO, que es el reverso
        // exacto de lo que Cajero/Facturar descontó al vender. Todo se guarda en una sola
        // transacción: estado del pedido, stock de los insumos y movimientos.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id, string? desde, string? hasta)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var volver = () => RedirectToAction(nameof(Index), new { desde, hasta });

            var pedido = await _context.Pedidos
                .Include(p => p.DetallesPedidos)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Receta)
                            .ThenInclude(r => r.DetalleRecetas)
                                .ThenInclude(dr => dr.Insumo)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null)
            {
                TempData["Error"] = "El pedido no existe.";
                return volver();
            }

            if (pedido.Estado == "CANCELADO")
            {
                TempData["Error"] = $"El pedido #{pedido.IdPedido:D4} ya estaba cancelado.";
                return volver();
            }

            // Solo se anulan ventas: un pedido sin método de pago es una comanda de cocina.
            if (string.IsNullOrWhiteSpace(pedido.EstadoPago) || pedido.EstadoPago == "PENDIENTE")
            {
                TempData["Error"] = $"El pedido #{pedido.IdPedido:D4} no está facturado, no se puede cancelar como venta.";
                return volver();
            }

            // El movimiento necesita un empleado válido (llave foránea), igual que en los ajustes.
            if (!int.TryParse(HttpContext.Session.GetString("UsuarioId"), out var idEmpleado) ||
                !await _context.Empleados.AnyAsync(e => e.IdEmpleado == idEmpleado))
            {
                TempData["Error"] = "No se pudo identificar al administrador de la sesión.";
                return volver();
            }

            var numeroFactura = string.IsNullOrWhiteSpace(pedido.NumeroPedido)
                ? $"#{pedido.IdPedido:D4}"
                : $"factura {pedido.NumeroPedido}";

            var movimientos = 0;

            // Devolver al stock lo que la receta consumió: cantidad_receta x unidades del pedido.
            foreach (var detalle in pedido.DetallesPedidos)
            {
                var detalleRecetas = detalle.Producto?.Receta?.DetalleRecetas;
                if (detalleRecetas == null) continue;

                foreach (var dr in detalleRecetas)
                {
                    if (dr.IdInsumo == 0 || dr.Insumo == null) continue;

                    var totalInsumo = dr.Cantidad * detalle.Cantidad;
                    if (totalInsumo <= 0) continue;

                    dr.Insumo.StockActual += totalInsumo;

                    _context.MovimientosInventario.Add(new MovimientoInventario
                    {
                        IdInsumo = dr.IdInsumo,
                        IdEmpleado = idEmpleado,
                        TipoMovimiento = "INGRESO",
                        Cantidad = totalInsumo,
                        Fecha = DateTime.Now,
                        Motivo = $"Cancelación del pedido {numeroFactura}: "
                               + $"{detalle.Cantidad:0.##} x {detalle.Producto?.Nombre}"
                    });
                    movimientos++;
                }
            }

            pedido.Estado = "CANCELADO";

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Pedido {numeroFactura} cancelado. "
                + (movimientos == 0
                    ? "No tenía insumos en receta que devolver."
                    : $"{movimientos} insumo(s) volvieron al inventario.");
            return volver();
        }

        // Carga el pedido con todo lo necesario para el detalle y la factura
        private async Task<Pedido?> CargarPedido(int id)
        {
            return await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Empleado)
                .Include(p => p.Cliente)
                .Include(p => p.Mesa)
                .Include(p => p.DetallesPedidos)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.IdPedido == id);
        }

        // Interpreta el rango de fechas pedido por la pantalla. Sin fechas = todo el historial.
        private static (DateTime? desde, DateTime? hasta, string periodo) ResolverRango(string? desde, string? hasta)
        {
            var hoy = DateTime.Now.Date;
            var okDesde = DateTime.TryParse(desde, out var dDesde);
            var okHasta = DateTime.TryParse(hasta, out var dHasta);

            if (!okDesde && !okHasta) return (null, null, "Todo el historial");

            if (!okDesde) dDesde = hoy;
            if (!okHasta) dHasta = hoy;

            if (dDesde > dHasta) (dDesde, dHasta) = (dHasta, dDesde);

            string periodo = dDesde == dHasta
                ? $"Día {dDesde:dd/MM/yyyy}"
                : $"{dDesde:dd/MM/yyyy} al {dHasta:dd/MM/yyyy}";

            return (dDesde.Date, dHasta.Date, periodo);
        }
    }
}
