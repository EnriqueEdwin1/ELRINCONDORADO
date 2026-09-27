using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Helpers;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class ProductosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string? _imgbbApiKey;

        public ProductosController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _imgbbApiKey = configuration["ImgBB:ApiKey"];
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

        private void CargarListas(int? idCategoria = null)
        {
            ViewBag.IdCategoria = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.Categorias.OrderBy(c => c.Nombre).ToList(), "IdCategoria", "Nombre", idCategoria);
            ViewBag.Insumos = _context.Insumos.Where(i => i.Activo).OrderBy(i => i.Nombre).ToList();
        }

        private List<DetalleRecetaViewModel> ObtenerDetallesValidos(List<DetalleRecetaViewModel>? detalles)
        {
            return detalles
                ?.Where(d => d.IdInsumo > 0 && d.Cantidad > 0)
                .ToList() ?? new List<DetalleRecetaViewModel>();
        }

        private const string VistaProductos = "~/Views/Administrador/Productos/Index.cshtml";
        private const string VistaDetails = "~/Views/Administrador/Productos/Details.cshtml";
        private const string VistaCreate = "~/Views/Administrador/Productos/Create.cshtml";
        private const string VistaEdit = "~/Views/Administrador/Productos/Edit.cshtml";
        private const string VistaDelete = "~/Views/Administrador/Productos/Delete.cshtml";
        private const string VistaPromocionForm = "~/Views/Administrador/Productos/PromocionForm.cshtml";
        private const string VistaPromocionDetails = "~/Views/Administrador/Productos/PromocionDetails.cshtml";
        private const string VistaPromocionDelete = "~/Views/Administrador/Productos/PromocionDelete.cshtml";

        // ===== PROMOCIONES =====

        // Productos que se pueden incluir en una promoción: cualquier producto del catálogo
        private async Task<List<ProductoPromocionItem>> ObtenerCandidatosAsync()
        {
            return await _context.Productos
                .OrderBy(p => p.Nombre)
                .Select(p => new ProductoPromocionItem
                {
                    IdProducto = p.IdProducto,
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    Activo = p.Activo,
                    Cantidad = 1
                })
                .ToListAsync();
        }

        private async Task<PromocionFormViewModel> ConstruirFormularioPromocionAsync(Promocion? promocion)
        {
            var modelo = new PromocionFormViewModel();
            if (promocion != null)
            {
                modelo.IdPromocion = promocion.IdPromocion;
                modelo.Nombre = promocion.Nombre;
                modelo.Descripcion = promocion.Descripcion;
                modelo.Precio = promocion.Valor;
                modelo.Estado = promocion.Estado;
                modelo.ImagenActual = promocion.ImagenUrl;
                modelo.ImagenActualDeleteUrl = promocion.DeleteUrl;
            }

            var cantidades = new Dictionary<int, int>();
            if (promocion != null)
            {
                var filas = await _context.DetallePromociones
                    .Where(dp => dp.IdPromocion == promocion.IdPromocion)
                    .ToListAsync();

                foreach (var fila in filas)
                    cantidades[fila.IdProducto] = fila.Cantidad;
            }

            modelo.Incluidos = await ObtenerCandidatosAsync();
            foreach (var item in modelo.Incluidos)
            {
                if (cantidades.TryGetValue(item.IdProducto, out var cant))
                {
                    item.Seleccionado = true;
                    item.Cantidad = cant;
                }
            }

            return modelo;
        }

        private static List<ProductoPromocionItem> ObtenerIncluidosValidos(List<ProductoPromocionItem>? incluidos)
        {
            return incluidos?
                .Where(i => i.Seleccionado && i.IdProducto > 0 && i.Cantidad > 0)
                .ToList() ?? new List<ProductoPromocionItem>();
        }

        // Reemplaza los productos incluidos (detalle_promociones) por los seleccionados
        private async Task ActualizarDetallePromocionAsync(int idPromocion, List<ProductoPromocionItem> incluidos)
        {
            var actuales = await _context.DetallePromociones
                .Where(dp => dp.IdPromocion == idPromocion)
                .ToListAsync();

            var cantidades = incluidos.ToDictionary(i => i.IdProducto, i => i.Cantidad);

            foreach (var actual in actuales.Where(a => !cantidades.ContainsKey(a.IdProducto)).ToList())
                _context.DetallePromociones.Remove(actual);

            foreach (var fila in actuales.Where(a => cantidades.ContainsKey(a.IdProducto)))
                fila.Cantidad = cantidades[fila.IdProducto];

            foreach (var item in incluidos.Where(i => actuales.All(a => a.IdProducto != i.IdProducto)))
            {
                _context.DetallePromociones.Add(new DetallePromocion
                {
                    IdPromocion = idPromocion,
                    IdProducto = item.IdProducto,
                    Cantidad = item.Cantidad
                });
            }
        }

        private async Task<List<PromocionListadoViewModel>> ConstruirListadoPromocionesAsync()
        {
            var promociones = await _context.Promociones
                .Include(p => p.DetallePromociones!)
                    .ThenInclude(dp => dp.Producto)
                .OrderByDescending(p => p.IdPromocion)
                .ToListAsync();

            return promociones.Select(p =>
            {
                var incluidos = p.DetallePromociones!.Where(dp => dp.Producto != null).ToList();

                return new PromocionListadoViewModel
                {
                    Promocion = p,
                    SumaProductos = incluidos.Sum(dp => dp.Producto!.Precio * dp.Cantidad),
                    CantidadProductos = incluidos.Count,
                    ResumenProductos = string.Join(", ", incluidos.Select(dp => $"{dp.Producto!.Nombre} x{dp.Cantidad}"))
                };
            }).ToList();
        }


        // GET: Productos/PromocionCreate
        public async Task<IActionResult> PromocionCreate()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            return View(VistaPromocionForm, await ConstruirFormularioPromocionAsync(null));
        }

        // POST: Productos/PromocionCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromocionCreate(PromocionFormViewModel modelo, IFormFile? imagenFile)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            return await GuardarPromocionAsync(null, modelo, esCreacion: true, imagenFile);
        }

        // GET: Productos/PromocionEdit/5
        public async Task<IActionResult> PromocionEdit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
                return NotFound();

            var promocion = await _context.Promociones
                .FirstOrDefaultAsync(p => p.IdPromocion == id);
            if (promocion == null)
                return NotFound();

            return View(VistaPromocionForm, await ConstruirFormularioPromocionAsync(promocion));
        }

        // POST: Productos/PromocionEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromocionEdit(int id, PromocionFormViewModel modelo, IFormFile? imagenFile)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id != modelo.IdPromocion)
                return NotFound();

            return await GuardarPromocionAsync(id, modelo, esCreacion: false, imagenFile);
        }

        // POST: Productos/PromocionEliminarImagen/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromocionEliminarImagen(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var promocion = await _context.Promociones
                .FirstOrDefaultAsync(p => p.IdPromocion == id);
            if (promocion == null)
                return NotFound();

            await ImgbbHelper.EliminarImagenAsync(promocion.DeleteUrl);
            promocion.ImagenUrl = null;
            promocion.DisplayUrl = null;
            promocion.DeleteUrl = null;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Imagen de la promoción eliminada correctamente.";
            return RedirectToAction(nameof(PromocionEdit), new { id });
        }

        // GET: Productos/PromocionDetails/5
        public async Task<IActionResult> PromocionDetails(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
                return NotFound();

            var modelo = await ConstruirDetallePromocionAsync(id.Value);
            if (modelo == null)
                return NotFound();

            return View(VistaPromocionDetails, modelo);
        }

        // GET: Productos/PromocionDelete/5
        public async Task<IActionResult> PromocionDelete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
                return NotFound();

            var promocion = await _context.Promociones
                .Include(p => p.DetallePromociones!)
                    .ThenInclude(dp => dp.Producto)
                .FirstOrDefaultAsync(p => p.IdPromocion == id);
            if (promocion == null)
                return NotFound();

            var modelo = ConstruirDetalleDesdeEntidad(promocion);
            return View(VistaPromocionDelete, modelo);
        }

        // POST: Productos/PromocionDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromocionDelete(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var promocion = await _context.Promociones
                .FirstOrDefaultAsync(p => p.IdPromocion == id);
            if (promocion == null)
                return NotFound();

            // No se permite borrar una promoción que ya se usó en pedidos facturados
            var usada = await _context.Pedidos.AnyAsync(p => p.IdPromocion == id);
            if (usada)
            {
                TempData["Error"] = "La promoción ya se usó en pedidos facturados, por lo que no se puede eliminar. Puedes pasarla a INACTIVA.";
                return RedirectToAction(nameof(Index), new { pestana = "promociones" });
            }

            await ImgbbHelper.EliminarImagenAsync(promocion.DeleteUrl);

            _context.DetallePromociones.RemoveRange(
                await _context.DetallePromociones.Where(dp => dp.IdPromocion == id).ToListAsync());
            _context.Promociones.Remove(promocion);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Promoción eliminada correctamente.";
            return RedirectToAction(nameof(Index), new { pestana = "promociones" });
        }

        private async Task<IActionResult> GuardarPromocionAsync(int? id, PromocionFormViewModel modelo, bool esCreacion, IFormFile? imagenFile = null)
        {
            modelo.Incluidos ??= new List<ProductoPromocionItem>();

            // Nombres y precios se toman de la base: el formulario solo manda el checkbox y la cantidad
            var reales = await _context.Productos
                .Select(p => new { p.IdProducto, p.Nombre, p.Precio })
                .ToListAsync();

            foreach (var item in modelo.Incluidos)
            {
                var real = reales.FirstOrDefault(r => r.IdProducto == item.IdProducto);
                if (real == null) continue;
                item.Nombre = real.Nombre;
                item.Precio = real.Precio;
            }

            var seleccionados = ObtenerIncluidosValidos(modelo.Incluidos)
                .Where(s => reales.Any(r => r.IdProducto == s.IdProducto))
                .ToList();

            if (string.IsNullOrWhiteSpace(modelo.Nombre))
                ModelState.AddModelError(nameof(modelo.Nombre), "El nombre de la promoción es obligatorio.");

            if (seleccionados.Count == 0)
                ModelState.AddModelError(string.Empty, "Selecciona al menos un producto para la promoción.");

            if (modelo.Precio <= 0)
                ModelState.AddModelError(nameof(modelo.Precio), "El precio de la promoción debe ser mayor a cero.");

            // El cajero cobra la suma de los productos y descuenta la diferencia, así que un
            // precio mayor que esa suma no se podría representar como descuento.
            var sumaProductos = seleccionados.Sum(s => s.Precio * s.Cantidad);
            if (modelo.Precio > sumaProductos)
                ModelState.AddModelError(nameof(modelo.Precio), $"El precio no puede superar la suma de los productos (Bs {sumaProductos.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}).");


            if (modelo.Estado != "ACTIVA" && modelo.Estado != "INACTIVA")
                modelo.Estado = "ACTIVA";

            if (!ModelState.IsValid)
                return View(VistaPromocionForm, modelo);

            Promocion promocion;
            if (esCreacion)
            {
                promocion = new Promocion
                {
                    Nombre = modelo.Nombre.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(modelo.Descripcion) ? null : modelo.Descripcion.Trim(),
                    Tipo = "PAQUETE",
                    Valor = modelo.Precio,
                    FechaInicio = DateTime.Today,
                    FechaFin = DateTime.Today.AddYears(5),
                    Estado = modelo.Estado
                };

                _context.Promociones.Add(promocion);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existente = await _context.Promociones.FindAsync(id);
                if (existente == null)
                    return NotFound();

                existente.Nombre = modelo.Nombre.Trim();
                existente.Descripcion = string.IsNullOrWhiteSpace(modelo.Descripcion) ? null : modelo.Descripcion.Trim();
                existente.Valor = modelo.Precio;
                existente.Estado = modelo.Estado;
                promocion = existente;
            }

            // Imagen: si viene un archivo nuevo se sube a ImgBB y se borra la anterior
            if (imagenFile != null && imagenFile.Length > 0)
            {
                var imagen = await ImgbbHelper.SubirImagenAsync(imagenFile, _imgbbApiKey ?? "");
                if (imagen?.Url == null)
                {
                    ModelState.AddModelError(string.Empty, "No se pudo subir la imagen a ImgBB. Verifica el archivo y vuelve a intentarlo.");
                }
                else
                {
                    var anterior = promocion.DeleteUrl;
                    promocion.ImagenUrl = imagen.Url;
                    promocion.DisplayUrl = imagen.DisplayUrl;
                    promocion.DeleteUrl = imagen.DeleteUrl;
                    if (!string.IsNullOrWhiteSpace(anterior))
                        await ImgbbHelper.EliminarImagenAsync(anterior);
                }
            }

            await ActualizarDetallePromocionAsync(promocion.IdPromocion, seleccionados);
            await _context.SaveChangesAsync();

            if (!ModelState.IsValid)
                return View(VistaPromocionForm, modelo);

            if (esCreacion)
            {
                var resumen = new PromocionListadoViewModel
                {
                    Promocion = promocion,
                    SumaProductos = seleccionados.Sum(s => s.Precio * s.Cantidad)
                };

                TempData["Mensaje"] = resumen.PorcentajeAhorro > 0
                    ? $"Promoción creada. Ahorro del {resumen.PorcentajeAhorro.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}%."
                    : "Promoción creada correctamente.";
            }
            else
            {
                TempData["Mensaje"] = "Promoción actualizada correctamente.";
            }

            return RedirectToAction(nameof(Index), new { pestana = "promociones" });
        }

        // GET: Productos
        public async Task<IActionResult> Index(string? pestana)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            // Las promociones viven en su propia tabla (promociones), no en productos
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Receta)
                .OrderByDescending(p => p.IdProducto)
                .ToListAsync();

            var promociones = await ConstruirListadoPromocionesAsync();

            ViewBag.PestanaActiva = pestana == "promociones" ? "promociones" : "productos";
            ViewBag.TotalProductos = productos.Count;
            ViewBag.TotalPromociones = promociones.Count;
            ViewBag.Promociones = promociones;

            return View(VistaProductos, productos);
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Receta)
                    .ThenInclude(r => r.DetalleRecetas)
                        .ThenInclude(d => d.Insumo)
                .FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(VistaDetails, producto);
        }

        private async Task<PromocionDetalleViewModel?> ConstruirDetallePromocionAsync(int idPromocion)
        {
            var promocion = await _context.Promociones
                .Include(p => p.DetallePromociones!)
                    .ThenInclude(dp => dp.Producto)
                .FirstOrDefaultAsync(p => p.IdPromocion == idPromocion);

            return promocion == null ? null : ConstruirDetalleDesdeEntidad(promocion);
        }

        private static PromocionDetalleViewModel ConstruirDetalleDesdeEntidad(Promocion promocion)
        {
            var incluidos = (promocion.DetallePromociones ?? new List<DetallePromocion>())
                .Where(dp => dp.Producto != null)
                .OrderBy(dp => dp.Producto!.Nombre)
                .ToList();

            return new PromocionDetalleViewModel
            {
                Promocion = promocion,
                Incluidos = incluidos.Select(dp => new ProductoPromocionItem
                {
                    IdProducto = dp.IdProducto,
                    Nombre = dp.Producto!.Nombre,
                    Precio = dp.Producto!.Precio,
                    Activo = dp.Producto!.Activo,
                    Cantidad = dp.Cantidad
                }).ToList()
            };
        }

        // GET: Productos/Create
        public IActionResult Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            CargarListas();
            return View(VistaCreate, new ProductoRecetaViewModel());
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoRecetaViewModel modelo, IFormFile? imagenFile)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var producto = modelo.Producto;
            modelo.Receta ??= new RecetaViewModel();

            var detallesValidos = ObtenerDetallesValidos(modelo.Receta.Detalles);
            if (modelo.IncluirReceta && detallesValidos.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Agrega al menos un insumo con cantidad mayor a cero.");
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(producto.Codigo) &&
                    await _context.Productos.AnyAsync(p => p.Codigo == producto.Codigo))
                {
                    ModelState.AddModelError(string.Empty, "Ese código de producto ya está en uso.");
                    CargarListas(producto.IdCategoria);
                    return View(VistaCreate, modelo);
                }

                var nuevo = new Producto
                {
                    IdCategoria = producto.IdCategoria,
                    Codigo = producto.Codigo,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio,
                    Activo = producto.Activo
                };

                if (imagenFile != null && imagenFile.Length > 0)
                {
                    var imagen = await ImgbbHelper.SubirImagenAsync(imagenFile, _imgbbApiKey!);
                    if (imagen?.Url == null)
                    {
                        ModelState.AddModelError(string.Empty, "No se pudo subir la imagen a ImgBB. Verifica el archivo y vuelve a intentarlo.");
                        CargarListas(producto.IdCategoria);
                        return View(VistaCreate, modelo);
                    }

                    nuevo.ImagenUrl = imagen.Url;
                    nuevo.DisplayUrl = imagen.DisplayUrl;
                    nuevo.DeleteUrl = imagen.DeleteUrl;
                }

                if (modelo.IncluirReceta)
                {
                    var receta = new Receta
                    {
                        Descripcion = modelo.Receta.Descripcion,
                        Activo = modelo.Receta.Activo,
                        DetalleRecetas = new List<DetalleReceta>()
                    };

                    foreach (var detalle in detallesValidos)
                    {
                        var insumo = await _context.Insumos.FindAsync(detalle.IdInsumo);
                        if (insumo == null)
                            continue;

                        receta.DetalleRecetas.Add(new DetalleReceta
                        {
                            IdInsumo = insumo.IdInsumo,
                            Cantidad = detalle.Cantidad,
                            UnidadMedida = detalle.UnidadMedida ?? insumo.UnidadMedida
                        });
                    }

                    nuevo.Receta = receta;
                }

                _context.Productos.Add(nuevo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            CargarListas(producto.IdCategoria);
            return View(VistaCreate, modelo);
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Receta)
                    .ThenInclude(r => r.DetalleRecetas)
                        .ThenInclude(d => d.Insumo)
                .FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null)
            {
                return NotFound();
            }

            // Las promociones se editan en su propio formulario
            var modelo = new ProductoRecetaViewModel
            {
                Producto = producto,
                IncluirReceta = producto.Receta != null,
                Receta = new RecetaViewModel
                {
                    IdReceta = producto.Receta?.IdReceta ?? 0,
                    IdProducto = producto.IdProducto,
                    Descripcion = producto.Receta?.Descripcion,
                    Activo = producto.Receta?.Activo ?? true,
                    Detalles = producto.Receta?.DetalleRecetas?
                        .Select(d => new DetalleRecetaViewModel
                        {
                            IdDetalleReceta = d.IdDetalleReceta,
                            IdInsumo = d.IdInsumo,
                            Cantidad = d.Cantidad,
                            UnidadMedida = d.UnidadMedida
                        })
                        .ToList() ?? new List<DetalleRecetaViewModel>()
                }
            };

            CargarListas(producto.IdCategoria);
            return View(VistaEdit, modelo);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductoRecetaViewModel modelo, IFormFile? imagenFile)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var producto = modelo.Producto;
            modelo.Receta ??= new RecetaViewModel();

            if (id != producto.IdProducto)
            {
                return NotFound();
            }

            var existente = await _context.Productos.FindAsync(id);
            if (existente == null)
            {
                return NotFound();
            }

            // Las promociones se editan en su propio formulario
            var detallesValidos = ObtenerDetallesValidos(modelo.Receta.Detalles);
            if (modelo.IncluirReceta && detallesValidos.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Agrega al menos un insumo con cantidad mayor a cero.");
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(producto.Codigo) &&
                    await _context.Productos.AnyAsync(p => p.Codigo == producto.Codigo && p.IdProducto != producto.IdProducto))
                {
                    ModelState.AddModelError(string.Empty, "Ese código de producto ya está en uso.");
                    CargarListas(producto.IdCategoria);
                    return View(VistaEdit, modelo);
                }

                existente.IdCategoria = producto.IdCategoria;
                existente.Codigo = producto.Codigo;
                existente.Nombre = producto.Nombre;
                existente.Descripcion = producto.Descripcion;
                existente.Precio = producto.Precio;
                existente.Activo = producto.Activo;

                if (imagenFile != null && imagenFile.Length > 0)
                {
                    var imagen = await ImgbbHelper.SubirImagenAsync(imagenFile, _imgbbApiKey!);
                    if (imagen?.Url == null)
                    {
                        ModelState.AddModelError(string.Empty, "No se pudo subir la imagen a ImgBB. Verifica el archivo y vuelve a intentarlo.");
                        CargarListas(producto.IdCategoria);
                        return View(VistaEdit, modelo);
                    }

                    await ImgbbHelper.EliminarImagenAsync(existente.DeleteUrl);
                    existente.ImagenUrl = imagen.Url;
                    existente.DisplayUrl = imagen.DisplayUrl;
                    existente.DeleteUrl = imagen.DeleteUrl;
                }

                var receta = await _context.Recetas
                    .Include(r => r.DetalleRecetas)
                    .FirstOrDefaultAsync(r => r.IdProducto == id);

                if (modelo.IncluirReceta)
                {
                    if (receta == null)
                    {
                        receta = new Receta
                        {
                            IdProducto = id,
                            Descripcion = modelo.Receta.Descripcion,
                            Activo = modelo.Receta.Activo,
                            DetalleRecetas = new List<DetalleReceta>()
                        };

                        foreach (var detalle in detallesValidos)
                        {
                            var insumo = await _context.Insumos.FindAsync(detalle.IdInsumo);
                            if (insumo == null)
                                continue;

                            receta.DetalleRecetas.Add(new DetalleReceta
                            {
                                IdInsumo = insumo.IdInsumo,
                                Cantidad = detalle.Cantidad,
                                UnidadMedida = detalle.UnidadMedida ?? insumo.UnidadMedida
                            });
                        }

                        _context.Recetas.Add(receta);
                    }
                    else
                    {
                        receta.Descripcion = modelo.Receta.Descripcion;
                        receta.Activo = modelo.Receta.Activo;

                        _context.DetalleRecetas.RemoveRange(receta.DetalleRecetas ?? new List<DetalleReceta>());
                        foreach (var detalle in detallesValidos)
                        {
                            var insumo = await _context.Insumos.FindAsync(detalle.IdInsumo);
                            if (insumo == null)
                                continue;

                            _context.DetalleRecetas.Add(new DetalleReceta
                            {
                                IdReceta = receta.IdReceta,
                                IdInsumo = insumo.IdInsumo,
                                Cantidad = detalle.Cantidad,
                                UnidadMedida = detalle.UnidadMedida ?? insumo.UnidadMedida
                            });
                        }
                    }
                }
                else if (receta != null)
                {
                    _context.DetalleRecetas.RemoveRange(receta.DetalleRecetas ?? new List<DetalleReceta>());
                    _context.Recetas.Remove(receta);
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.IdProducto))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            CargarListas(producto.IdCategoria);
            return View(VistaEdit, modelo);
        }

        // POST: Productos/EliminarImagen/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarImagen(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            await ImgbbHelper.EliminarImagenAsync(producto.DeleteUrl);
            producto.ImagenUrl = null;
            producto.DisplayUrl = null;
            producto.DeleteUrl = null;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Imagen eliminada correctamente.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        // GET: Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Receta)
                .FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(VistaDelete, producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (await _context.DetallesPedidos.AnyAsync(d => d.IdProducto == id) ||
                     await _context.DetallePromociones.AnyAsync(d => d.IdProducto == id) ||
                     await _context.Recetas.AnyAsync(r => r.IdProducto == id))
            {
                TempData["Error"] = "No se puede eliminar: el producto tiene pedidos, promociones o una receta asociada.";
                return RedirectToAction(nameof(Index));
            }

            await ImgbbHelper.EliminarImagenAsync(producto.DeleteUrl);
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.IdProducto == id);
        }
    }
}
