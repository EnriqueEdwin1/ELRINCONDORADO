using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Helpers;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class ComprasController : Controller
    {
        private readonly AppDbContext _context;

        public ComprasController(AppDbContext context)
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

        private int? EmpleadoActual()
        {
            var id = HttpContext.Session.GetString("UsuarioId");
            return int.TryParse(id, out var resultado) ? resultado : null;
        }

        // GET: Compras
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var compras = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Empleado)
                .Include(c => c.DetallesCompras)
                .OrderByDescending(c => c.FechaCompra)
                .ToListAsync();

            return View("~/Views/Administrador/Compras/Index.cshtml", compras);
        }

        // GET: Compras/Create
        public IActionResult Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            CargarListas();
            return View("~/Views/Administrador/Compras/Create.cshtml", new CompraViewModel());
        }

        // POST: Compras/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompraViewModel modelo)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var idEmpleado = EmpleadoActual();
            if (idEmpleado == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var detallesValidos = modelo.Detalles
                ?.Where(d => d.IdInsumo > 0 && d.Cantidad > 0)
                .ToList() ?? new List<DetalleCompraViewModel>();

            if (detallesValidos.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Agrega al menos un insumo con cantidad mayor a cero.");
            }

            if (modelo.Descuento < 0 || modelo.Descuento > modelo.Subtotal)
            {
                ModelState.AddModelError(string.Empty, "El descuento no puede ser negativo ni mayor al subtotal.");
            }

            if (ModelState.IsValid)
            {
                var proveedor = await _context.Proveedores.FindAsync(modelo.IdProveedor);
                if (proveedor == null)
                {
                    ModelState.AddModelError(string.Empty, "Selecciona un proveedor válido.");
                }
                else
                {
                    var compra = new Compra
                    {
                        IdProveedor = modelo.IdProveedor,
                        IdEmpleado = idEmpleado.Value,
                        FechaCompra = DateTime.Now,
                        Subtotal = detallesValidos.Sum(d => d.Subtotal),
                        Descuento = modelo.Descuento,
                        Total = detallesValidos.Sum(d => d.Subtotal) - modelo.Descuento,
                        Estado = "REGISTRADA"
                    };

                    _context.Compras.Add(compra);
                    await _context.SaveChangesAsync();

                    foreach (var detalle in detallesValidos)
                    {
                        var insumo = await _context.Insumos.FindAsync(detalle.IdInsumo);
                        if (insumo == null)
                            continue;

                        // Aumentar el stock del insumo
                        insumo.StockActual += detalle.Cantidad;
                        insumo.CostoUnitario = detalle.CostoUnitario;

                        _context.DetallesCompras.Add(new DetalleCompra
                        {
                            IdCompra = compra.IdCompra,
                            IdInsumo = detalle.IdInsumo,
                            Cantidad = detalle.Cantidad,
                            CostoUnitario = detalle.CostoUnitario,
                            Subtotal = detalle.Subtotal
                        });

                        // Registrar el movimiento de inventario (entrada por compra)
                        _context.MovimientosInventario.Add(new MovimientoInventario
                        {
                            IdInsumo = detalle.IdInsumo,
                            IdEmpleado = idEmpleado.Value,
                            TipoMovimiento = "ENTRADA",
                            Cantidad = detalle.Cantidad,
                            Fecha = DateTime.Now,
                            Motivo = $"COMPRA #{compra.IdCompra}"
                        });
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { id = compra.IdCompra });
                }
            }

            CargarListas(modelo?.IdProveedor ?? 0);
            return View("~/Views/Administrador/Compras/Create.cshtml", modelo);
        }

        // POST: Compras/CrearInsumoJson -> crea un insumo desde el modal de Nueva Compra
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearInsumoJson(
            string? nombre, string? descripcion, string? unidadMedida,
            string? costoUnitario, string? stockActual, string? stockMinimo,
            string? activo, string? idDestino)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var opciones = new JsonSerializerOptions { PropertyNamingPolicy = null };
            var nombresPermitidos = new HashSet<string> { "Unidad", "Kilogramo", "Litros" };

            var insumo = new Insumo
            {
                Nombre = nombre?.Trim() ?? "",
                Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim(),
                UnidadMedida = unidadMedida?.Trim() ?? "",
                StockActual = ParseInv(stockActual) ?? 0,
                StockMinimo = ParseInv(stockMinimo) ?? 0,
                CostoUnitario = ParseInv(costoUnitario) ?? 0,
                Activo = bool.TryParse(activo, out var a) ? a : false,
                IdDestino = int.TryParse(idDestino, out var d) ? d : 1
            };

            if (string.IsNullOrWhiteSpace(insumo.Nombre))
                return Json(new { ok = false, error = "Ingresa el nombre del insumo." }, opciones);

            if (!nombresPermitidos.Contains(insumo.UnidadMedida))
                return Json(new { ok = false, error = "Selecciona una unidad de medida válida." }, opciones);

            if (insumo.CostoUnitario < 0)
                return Json(new { ok = false, error = "El costo unitario no puede ser negativo." }, opciones);

            if (insumo.StockActual < 0 || insumo.StockMinimo < 0)
                return Json(new { ok = false, error = "El stock no puede ser negativo." }, opciones);

            if (await _context.DestinosInsumos.AnyAsync(x => x.IdDestino == insumo.IdDestino) == false)
                insumo.IdDestino = 1;

            if (await _context.Insumos.AnyAsync(i => i.Nombre == insumo.Nombre))
                return Json(new { ok = false, error = "Ese nombre de insumo ya existe." }, opciones);

            _context.Add(insumo);
            await _context.SaveChangesAsync();

            return Json(new
            {
                ok = true,
                insumo = new
                {
                    IdInsumo = insumo.IdInsumo,
                    Nombre = insumo.Nombre,
                    UnidadMedida = insumo.UnidadMedida,
                    CostoUnitario = insumo.CostoUnitario
                }
            }, opciones);
        }

        // GET: Compras/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var compra = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Empleado)
                .Include(c => c.DetallesCompras)
                    .ThenInclude(d => d.Insumo)
                .FirstOrDefaultAsync(m => m.IdCompra == id);
            if (compra == null)
            {
                return NotFound();
            }

            return View("~/Views/Administrador/Compras/Details.cshtml", compra);
        }

        // GET: Compras/Comprobante/5 -> genera el PDF de la compra
        public async Task<IActionResult> Comprobante(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var compra = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Empleado)
                .Include(c => c.DetallesCompras)
                    .ThenInclude(d => d.Insumo)
                .FirstOrDefaultAsync(m => m.IdCompra == id);
            if (compra == null)
            {
                return NotFound();
            }

            var pdf = ComprobantePdfHelper.Generar(compra);
            return File(pdf, "application/pdf", $"Comprobante_Compra_{compra.IdCompra:D4}.pdf");
        }

        // GET: Compras/Reporte?tipo=dia&fecha=yyyy-MM-dd  o  Compras/Reporte?tipo=mes&fecha=yyyy-MM
        public async Task<IActionResult> Reporte(string tipo, string fecha)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            DateTime inicio;
            DateTime fin;
            string titulo;
            string sufijo;

            if (tipo == "mes")
            {
                if (!DateTime.TryParseExact(fecha, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var mes))
                    return BadRequest("Mes inválido.");
                inicio = new DateTime(mes.Year, mes.Month, 1);
                fin = inicio.AddMonths(1).AddTicks(-1);
                titulo = "Reporte mensual de compras";
                sufijo = inicio.ToString("yyyy-MM");
            }
            else
            {
                if (!DateTime.TryParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dia))
                    return BadRequest("Fecha inválida.");
                inicio = dia.Date;
                fin = inicio.AddDays(1).AddTicks(-1);
                titulo = "Reporte diario de compras";
                sufijo = inicio.ToString("yyyy-MM-dd");
            }

            var compras = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Empleado)
                .Include(c => c.DetallesCompras)
                    .ThenInclude(d => d.Insumo)
                .Where(c => c.FechaCompra >= inicio && c.FechaCompra <= fin)
                .OrderBy(c => c.FechaCompra)
                .ToListAsync();

            var pdf = ReporteComprasPdfHelper.Generar(compras, inicio, fin, titulo);
            return File(pdf, "application/pdf", $"Reporte_Compras_{sufijo}.pdf");
        }

        // GET: Compras/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var compra = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Empleado)
                .Include(c => c.DetallesCompras)
                    .ThenInclude(d => d.Insumo)
                .FirstOrDefaultAsync(m => m.IdCompra == id);
            if (compra == null)
            {
                return NotFound();
            }

            return View("~/Views/Administrador/Compras/Delete.cshtml", compra);
        }

        // POST: Compras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var compra = await _context.Compras
                .Include(c => c.DetallesCompras)
                .FirstOrDefaultAsync(c => c.IdCompra == id);
            if (compra == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Revertir el stock y eliminar los movimientos de inventario asociados
            foreach (var detalle in compra.DetallesCompras ?? new List<DetalleCompra>())
            {
                var insumo = await _context.Insumos.FindAsync(detalle.IdInsumo);
                if (insumo != null)
                {
                    insumo.StockActual -= detalle.Cantidad;
                }

                var movimientos = _context.MovimientosInventario
                    .Where(m => m.IdInsumo == detalle.IdInsumo && m.Motivo == $"COMPRA #{id}");
                _context.MovimientosInventario.RemoveRange(movimientos);
            }

            _context.DetallesCompras.RemoveRange(compra.DetallesCompras ?? new List<DetalleCompra>());
            _context.Compras.Remove(compra);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool CompraExists(int id)
        {
            return _context.Compras.Any(e => e.IdCompra == id);
        }

        private void CargarListas(int? idProveedor = null)
        {
            ViewBag.IdProveedor = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Proveedores, "IdProveedor", "Nombre", idProveedor);
            ViewBag.Insumos = _context.Insumos.OrderBy(i => i.Nombre).ToList();
            ViewBag.Destinos = _context.DestinosInsumos.OrderBy(d => d.Nombre).ToList();
        }

        // Convierte a decimal un valor recibido del formulario (notación invariante), o null si no es válido
        private static decimal? ParseInv(string? valor)
        {
            return decimal.TryParse(valor, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var resultado) ? resultado : null;
        }
    }
}