using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;
using ELRINCONDORADO.Helpers;
using ELRINCONDORADO.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Cryptography;
using System.Text;

namespace ELRINCONDORADO.Controllers
{
    public class CajeroController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<PedidosHub> _hubContext;

        public CajeroController(AppDbContext context, IHubContext<PedidosHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Verifica que haya sesión y que el rol sea CAJERO
        private IActionResult? ValidarAcceso()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Rol") != "CAJERO")
                return StatusCode(403, "Solo el cajero puede acceder a esta sección.");

            return null;
        }

        private int? ObtenerIdEmpleado()
        {
            var s = HttpContext.Session.GetString("UsuarioId");
            return int.TryParse(s, out var id) ? id : (int?)null;
        }

        // POS: menú horizontal de categorías -> productos (foto grande) + carrito
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var categorias = _context.Categorias
                .Include(c => c.Productos.Where(p => p.Activo))
                .OrderBy(c => c.IdCategoria);

            var promociones = await _context.Promociones
                .Include(p => p.DetallePromociones)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.Estado == "ACTIVA")
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            var viewModel = new CajeroIndexViewModel
            {
                Categorias = await categorias.ToListAsync(),
                Promociones = promociones,
                Mesas = await _context.Mesas
                    .Where(m => m.Estado == "DISPONIBLE")
                    .OrderBy(m => m.Numero)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Cajero/BuscarCliente?nit=... -> busca un cliente por NIT y devuelve su razón social
        public async Task<IActionResult> BuscarCliente(string? nit)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (string.IsNullOrWhiteSpace(nit))
                return Json(new { ok = false, mensaje = "Ingrese un NIT para buscar." });

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Nit == nit.Trim());
            if (cliente == null)
                return Json(new { ok = false, mensaje = "No está registrado. Complete la razón social." });

            // El cliente id=1 (NIT "0" - "SIN NOMBRE") queda bloqueado: nadie puede modificar su razón social
            return Json(new { ok = true, razonSocial = cliente.RazonSocial, bloqueado = cliente.IdCliente == 1 });
        }

        // POST: Cajero/Facturar -> recibe el carrito (producto+cantidad) + método de pago + datos de
        // factura; crea el Pedido PENDIENTE (lo ven Cocina, Administrador y Cajero), registra sus
        // DetallesPedidos y descuenta de los insumos la cantidad de la RECETA de cada producto
        // (cantidad_receta x unidades vendidas), dejando un MovimientoInventario tipo VENTA por insumo.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Facturar([FromBody] FacturaViewModel modelo)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (modelo.Items == null || modelo.Items.Count == 0)
            {
                return Json(new { ok = false, mensaje = "Debe seleccionar al menos un producto para facturar." });
            }

            if (string.IsNullOrWhiteSpace(modelo.NombrePedido))
            {
                return Json(new { ok = false, mensaje = "Indique el nombre del pedido/cliente." });
            }

            // Quitar líneas sin cantidad o sin producto
            var items = modelo.Items
                .Where(i => (i.IdProducto > 0 || i.IdPromocion > 0) && i.Cantidad > 0)
                .ToList();
            if (items.Count == 0)
            {
                return Json(new { ok = false, mensaje = "Debe seleccionar al menos un producto o promoción con cantidad mayor a 0." });
            }

            var idsProductos = items.Where(i => i.IdProducto > 0).Select(i => i.IdProducto).ToList();
            var idsPromociones = items.Where(i => i.IdPromocion > 0).Select(i => i.IdPromocion).Distinct().ToList();

            // Las promociones se venden como una línea del carrito con su propio precio, pero el
            // pedido guarda los productos que las componen para descontar stock y que la cocina los vea.
            var promociones = await _context.Promociones
                .Include(p => p.DetallePromociones!)
                    .ThenInclude(dp => dp.Producto)
                .Where(p => idsPromociones.Contains(p.IdPromocion) && p.Estado == "ACTIVA")
                .ToListAsync();

            // Se cargan también los productos que componen las promociones para poder descontar su stock.
            var idsProductosACargar = idsProductos
                .Concat(promociones.SelectMany(p => p.DetallePromociones ?? new List<DetallePromocion>())
                    .Where(dp => dp.Producto != null)
                    .Select(dp => dp.IdProducto))
                .Distinct()
                .ToList();

            var productos = await _context.Productos
                .Include(p => p.Receta)
                    .ThenInclude(r => r.DetalleRecetas)
                        .ThenInclude(d => d.Insumo)
                .Where(p => idsProductosACargar.Contains(p.IdProducto))
                .ToListAsync();

            var idEmpleado = ObtenerIdEmpleado();
            decimal subtotal = 0;
            decimal descuento = 0;
            int? idPromocionPedido = null;
            var detalles = new List<DetallePedido>();
            var totalesInsumo = new Dictionary<int, decimal>();

            // Acumula cantidad y precio por producto: una promoción y un producto pedido por
            // separado terminan en una sola línea del pedido.
            var lineas = new Dictionary<int, (int Cantidad, decimal Precio)>();

            foreach (var item in items)
            {
                if (item.IdPromocion > 0)
                {
                    var promocion = promociones.FirstOrDefault(p => p.IdPromocion == item.IdPromocion);
                    if (promocion == null)
                    {
                        return Json(new { ok = false, mensaje = "Hay una promoción inválida o que ya no está activa en la factura." });
                    }

                    var incluidos = (promocion.DetallePromociones ?? new List<DetallePromocion>())
                        .Where(dp => dp.Producto != null && dp.Cantidad > 0)
                        .ToList();

                    if (incluidos.Count == 0)
                    {
                        return Json(new { ok = false, mensaje = $"La promoción \"{promocion.Nombre}\" no tiene productos cargados." });
                    }

                    if (promocion.Valor <= 0)
                    {
                        return Json(new { ok = false, mensaje = $"La promoción \"{promocion.Nombre}\" no tiene un precio válido." });
                    }

                    idPromocionPedido ??= promocion.IdPromocion;

                    // Los productos entran a su precio de menú; la diferencia hasta el precio de la
                    // promoción se guarda como descuento del pedido.
                    foreach (var incluido in incluidos)
                    {
                        var productoIncluido = incluido.Producto!;
                        var cantidadTotal = incluido.Cantidad * item.Cantidad;
                        lineas[productoIncluido.IdProducto] = lineas.TryGetValue(productoIncluido.IdProducto, out var previa)
                            ? (previa.Cantidad + cantidadTotal, productoIncluido.Precio)
                            : (cantidadTotal, productoIncluido.Precio);
                    }

                    var valorMenu = incluidos.Sum(dp => dp.Producto!.Precio * dp.Cantidad) * item.Cantidad;
                    var valorPromocion = promocion.Valor * item.Cantidad;
                    subtotal += valorMenu;
                    descuento += valorMenu - valorPromocion;
                    continue;
                }

                var producto = productos.FirstOrDefault(p => p.IdProducto == item.IdProducto);
                if (producto == null || producto.Precio <= 0)
                {
                    return Json(new { ok = false, mensaje = "Hay un producto inválido o sin precio en la factura." });
                }

                lineas[producto.IdProducto] = lineas.TryGetValue(producto.IdProducto, out var previaProducto)
                    ? (previaProducto.Cantidad + item.Cantidad, producto.Precio)
                    : (item.Cantidad, producto.Precio);
                subtotal += producto.Precio * item.Cantidad;
            }

            if (descuento < 0) descuento = 0;

            foreach (var linea in lineas)
            {
                var producto = productos.FirstOrDefault(p => p.IdProducto == linea.Key);
                if (producto == null) continue;

                var cantidad = linea.Value.Cantidad;
                var precio = linea.Value.Precio;

                detalles.Add(new DetallePedido
                {
                    IdProducto = producto.IdProducto,
                    Cantidad = cantidad,
                    PrecioUnitario = precio,
                    Subtotal = cantidad * precio
                });

                // Descontar insumos según la receta del producto: cantidad_receta x unidades vendidas.
                // Los movimientos se acumulan por insumo dentro del turno abierto (un solo registro
                // por insumo cuyas cantidades van sumándose hasta el próximo cierre de caja).
                if (producto.Receta?.DetalleRecetas != null && idEmpleado.HasValue)
                {
                    foreach (var dr in producto.Receta.DetalleRecetas)
                    {
                        if (dr.IdInsumo == 0) continue;

                        var totalInsumo = dr.Cantidad * cantidad;
                        if (totalInsumo <= 0) continue;

                        var insumo = dr.Insumo;
                        if (insumo == null) continue;

                        insumo.StockActual -= totalInsumo;

                        totalesInsumo[dr.IdInsumo] = totalesInsumo.TryGetValue(dr.IdInsumo, out var ya)
                            ? ya + totalInsumo
                            : totalInsumo;
                    }
                }
            }

            if (detalles.Count == 0)
            {
                return Json(new { ok = false, mensaje = "El pedido no tiene productos para facturar." });
            }

            var estadoPago = string.IsNullOrWhiteSpace(modelo.MetodoPago)
                ? "EFECTIVO"
                : modelo.MetodoPago.Trim().ToUpperInvariant();

            // El cajero marca si el pedido es para llevar o para la mesa
            var tipoPedido = string.IsNullOrWhiteSpace(modelo.TipoPedido)
                ? "PARA_LLEVAR"
                : modelo.TipoPedido.Trim().ToUpperInvariant();
            int? idMesa = null;
            if (tipoPedido == "MESA")
            {
                if (modelo.IdMesa.HasValue && await _context.Mesas.AnyAsync(m => m.IdMesa == modelo.IdMesa.Value))
                {
                    idMesa = modelo.IdMesa.Value;
                }
                else
                {
                    return Json(new { ok = false, mensaje = "Seleccione el número de mesa para el pedido." });
                }
            }

            // El NIT/razón social se guardan en la tabla clientes (pedidos solo referencia por IdCliente).
            // Un cliente YA registrado se reutiliza tal cual (nunca se modifica su razón social); el
            // cliente id=1 (NIT "0" - "SIN NOMBRE") queda protegido de cualquier edición o duplicado.
            var nitCliente = (modelo.Nit ?? "").Trim();
            var razonSocialCliente = (modelo.RazonSocial ?? "").Trim().ToUpperInvariant();
            int? idCliente = null;
            if (!string.IsNullOrWhiteSpace(nitCliente) || !string.IsNullOrWhiteSpace(razonSocialCliente))
            {
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Nit == nitCliente);
                if (cliente == null)
                {
                    cliente = new Cliente
                    {
                        Nit = nitCliente,
                        RazonSocial = string.IsNullOrWhiteSpace(razonSocialCliente)
                            ? "SIN NOMBRE"
                            : razonSocialCliente
                    };
                    _context.Clientes.Add(cliente);
                    await _context.SaveChangesAsync();
                }
                idCliente = cliente.IdCliente;
            }

            var pedido = new Pedido
            {
                IdEmpleado = idEmpleado ?? 0,
                IdMesa = idMesa,
                IdPromocion = idPromocionPedido,
                TipoPedido = tipoPedido,
                Estado = "PENDIENTE",
                EstadoPago = estadoPago,
                FechaCreacion = DateTime.UtcNow,
                Subtotal = subtotal,
                Descuento = descuento,
                Total = subtotal - descuento,
                NombrePedido = modelo.NombrePedido.Trim().ToUpperInvariant(),
                IdCliente = idCliente,
                Observaciones = string.IsNullOrWhiteSpace(modelo.Observaciones)
                    ? null
                    : modelo.Observaciones.Trim(),
                DetallesPedidos = detalles
            };

            // Número de factura consecutivo: se asigna de forma ascendente y se reinicia
            // en 1 en cada cierre de caja (se guarda en la tabla configuracion).
            await AsignarNumeroFactura(pedido);

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            // Notificar a la cocina en tiempo real mediante SignalR
            await _hubContext.Clients.Group("Cocina").SendAsync("RecibirNuevoPedido", pedido.IdPedido);

            // Acumular las salidas del turno: si el insumo ya tiene un movimiento abierto (sin cierre),
            // se suma la cantidad al mismo registro; si no, se crea el registro de ese insumo.
            if (totalesInsumo.Count > 0 && idEmpleado.HasValue)
            {
                foreach (var kv in totalesInsumo)
                {
                    var mov = await _context.MovimientosInventario
                        .FirstOrDefaultAsync(m => m.IdCierre == null
                            && m.TipoMovimiento == "VENTA"
                            && m.IdInsumo == kv.Key);
                    if (mov != null)
                    {
                        mov.Cantidad += kv.Value;
                        mov.Fecha = DateTime.UtcNow;
                    }
                    else
                    {
                        _context.MovimientosInventario.Add(new MovimientoInventario
                        {
                            IdInsumo = kv.Key,
                            IdEmpleado = idEmpleado.Value,
                            TipoMovimiento = "VENTA",
                            Cantidad = kv.Value,
                            Fecha = DateTime.UtcNow,
                            Motivo = "VENTA - acumulado del turno"
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            var totalCobrado = subtotal - descuento;
            var detalleMensaje = descuento > 0
                ? $" con un descuento de promoción de {MonedaHelper.FormatearBs(descuento)}"
                : "";

            TempData["Exito"] = $"Pedido #{pedido.IdPedido} facturado por {MonedaHelper.FormatearBs(totalCobrado)}{detalleMensaje}. Stock descontado por receta.";
            TempData["IdPedidoFacturado"] = pedido.IdPedido.ToString();
            return Json(new
            {
                ok = true,
                idPedido = pedido.IdPedido,
                mensaje = $"Pedido #{pedido.IdPedido} facturado por {MonedaHelper.FormatearBs(totalCobrado)}{detalleMensaje}."
            });
        }

        // GET: Cajero/Factura/5 -> factura PDF descargable con el logo del restaurante
        public async Task<IActionResult> Factura(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var pedido = await _context.Pedidos
                .Include(p => p.Empleado)
                .Include(p => p.Cliente)
                .Include(p => p.Mesa)
                .Include(p => p.DetallesPedidos)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.IdPedido == id);
            if (pedido == null) return NotFound();

            var pdf = FacturaPdfHelper.Generar(pedido);
            return File(pdf, "application/pdf", $"Factura_Pedido_{pedido.IdPedido:D4}.pdf");
        }

        // GET: Cajero/FacturaVista/5 -> vista HTML de la factura (imprimir o descargar PDF)
        public async Task<IActionResult> FacturaVista(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var pedido = await _context.Pedidos
                .Include(p => p.Empleado)
                .Include(p => p.Cliente)
                .Include(p => p.Mesa)
                .Include(p => p.Promocion)
                .Include(p => p.DetallesPedidos)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.IdPedido == id);
            if (pedido == null) return NotFound();

            return View(pedido);
        }

        // GET: Cajero/HuellaVenta/5 -> ticket PDF opcional (mismo estilo, sin detalle de factura)
        public async Task<IActionResult> HuellaVenta(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.DetallesPedidos)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.IdPedido == id);
            if (pedido == null) return NotFound();

            var pdf = FacturaPdfHelper.Generar(pedido);
            return File(pdf, "application/pdf", $"Tickete_Venta_{pedido.IdPedido:D4}.pdf");
        }

        // Asigna el número de factura consecutivo (ascendente) al pedido y deja el contador
        // listo para el siguiente. El contador se reinicia en 1 en cada cierre de caja.
        private async Task AsignarNumeroFactura(Pedido pedido)
        {
            var clave = "proximo_numero_pedido";
            var cfg = await _context.Configuracion.FirstOrDefaultAsync(c => c.Clave == clave);
            int proximo = 1;
            if (cfg != null && int.TryParse(cfg.Valor, out var valor))
                proximo = valor;

            pedido.NumeroPedido = proximo.ToString("D4");

            var siguiente = proximo + 1;
            if (cfg == null)
                _context.Configuracion.Add(new Configuracion { Clave = clave, Valor = siguiente.ToString() });
            else
                cfg.Valor = siguiente.ToString();
        }

        // POST: Cajero/CierreCaja -> requiere la contraseña del administrador; cierra el turno:
        // guarda el corte en cierres_caja, vincula las salidas del turno a ese corte, reinicia la
        // numeración de facturas (empieza en 1) y muestra el reporte del día.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CierreCaja(string? passwordAdmin)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (!await ValidarPasswordAdministrador(passwordAdmin))
            {
                TempData["Error"] = "Contraseña de administrador incorrecta. No se realizó el cierre de caja.";
                return RedirectToAction(nameof(Index));
            }

            var vm = await ConstruirCierreCaja();

            // Guardar el corte: cada cierre es un "turno" que agrupa los movimientos que salieron
            var cierre = new CierreCaja
            {
                Fecha = DateTime.UtcNow,
                IdEmpleado = ObtenerIdEmpleado() ?? 0,
                TotalVentas = vm.Total
            };
            _context.CierresCaja.Add(cierre);
            await _context.SaveChangesAsync();

            // Vincular al corte recién creado todos los movimientos del turno que la pantalla de
            // Movimientos muestra: salidas por venta, ajustes e ingresos por cancelaciones.
            // (Las ENTRADA de las compras siguen fuera de esa pantalla, pero también se cierran
            //  aquí para que no queden pendientes de un turno ya cerrado.)
            var movimientos = await _context.MovimientosInventario
                .Where(m => m.IdCierre == null)
                .ToListAsync();
            foreach (var m in movimientos)
                m.IdCierre = cierre.IdCierre;

            // Reiniciar el contador: la siguiente factura (después del cierre) será la Nº 0001
            var clave = "proximo_numero_pedido";
            var cfg = await _context.Configuracion.FirstOrDefaultAsync(c => c.Clave == clave);
            if (cfg == null)
                _context.Configuracion.Add(new Configuracion { Clave = clave, Valor = "1" });
            else
                cfg.Valor = "1";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(CierreCajaReporte));
        }

        // GET: Cajero/CierreCajaReporte -> vista imprimible (hoja carta) con todas las ventas del día
        public async Task<IActionResult> CierreCajaReporte()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var vm = await ConstruirCierreCaja();
            return View("~/Views/Cajero/CierreCajaReporte.cshtml", vm);
        }

        // GET: Cajero/CierreCajaPdf -> reporte del día en PDF (tamaño hoja carta)
        public async Task<IActionResult> CierreCajaPdf()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var vm = await ConstruirCierreCaja();
            var pdf = CierreCajaPdfHelper.Generar(vm);
            return File(pdf, "application/pdf", $"CierreCaja_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        // Reúne todas las ventas (pedidos facturados) del día y calcula los totales por método de pago
        private async Task<CierreCajaViewModel> ConstruirCierreCaja()
        {
            var hoy = DateTime.Today;

            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Mesa)
                .Include(p => p.Empleado)
                .Include(p => p.DetallesPedidos)
                    .ThenInclude(d => d.Producto)
                .OrderBy(p => p.FechaCreacion)
                .ToListAsync();

            // Un pedido cancelado por el administrador no es una venta: no cuenta para el
            // total del turno ni para ningún método de pago del reporte.
            var ventas = pedidos
                .Where(p => p.Estado != "CANCELADO" && p.FechaCreacion.ToLocalTime().Date == hoy)
                .ToList();

            var vm = new CierreCajaViewModel
            {
                Ventas = ventas,
                Cajero = $"{HttpContext.Session.GetString("Nombre") ?? "Cajero"}",
                Fecha = DateTime.Now,
                Cantidad = ventas.Count,
                Subtotal = ventas.Sum(v => v.Subtotal),
                Total = ventas.Sum(v => v.Total),
                Efectivo = ventas.Where(v => v.EstadoPago == "EFECTIVO").Sum(v => v.Total),
                Tarjeta = ventas.Where(v => v.EstadoPago == "TARJETA").Sum(v => v.Total),
                QR = ventas.Where(v => v.EstadoPago == "QR").Sum(v => v.Total)
            };
            return vm;
        }

        // Verifica que la contraseña corresponda a la de algún administrador activo
        private async Task<bool> ValidarPasswordAdministrador(string? password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

            var admins = await _context.Empleados
                .Include(e => e.Rol)
                .Where(e => e.Rol != null && e.Rol.Nombre == "ADMINISTRADOR" && e.Estado == "ACTIVO")
                .ToListAsync();

            if (admins.Count == 0) return false;

            var passwordOk = admins.Any(a => VerifyPassword(password, a.PasswordHash));
            return passwordOk;
        }

        // Misma lógica que AuthController: acepta contraseña en claro o el hash almacenado
        private static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(hash)) return false;
            if (string.Equals(password, hash, StringComparison.Ordinal)) return true;
            return HashPassword(password) == hash;
        }

        private static string HashPassword(string password)
        {
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        }
    }
}
