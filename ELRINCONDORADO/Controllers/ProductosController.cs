using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ELRINCONDORADO.Services.Api;
using ELRINCONDORADO.Models;
using ELRINCONDORADO.Models.ApiDtos;
using ELRINCONDORADO.Helpers;

namespace ELRINCONDORADO.Controllers
{
    public partial class ProductosController : Controller
    {
        private readonly ProductosApiService _apiService;
        private readonly CategoriasApiService _categoriasApiService;
        private readonly PromocionesApiService _promocionesApiService;
        private readonly InsumosApiService _insumosApiService;
        private readonly IConfiguration _configuration;

        public ProductosController(
            ProductosApiService apiService,
            CategoriasApiService categoriasApiService,
            PromocionesApiService promocionesApiService,
            InsumosApiService insumosApiService,
            IConfiguration configuration)
        {
            _apiService = apiService;
            _categoriasApiService = categoriasApiService;
            _promocionesApiService = promocionesApiService;
            _insumosApiService = insumosApiService;
            _configuration = configuration;
        }

        // Sube la imagen a ImgBB si el formulario trae un archivo. Acepta
        // cualquier formato: el helper detecta el tipo real del archivo.
        // Devuelve (true, resultado, null) con la subida hecha, (true, null, null)
        // cuando no viene archivo, y (false, null, mensajeError) si falló.
        private async Task<(bool ok, ImgbbResult? resultado, string? error)> SubirImagenSiViene(IFormFile? imagenFile)
        {
            if (imagenFile == null || imagenFile.Length == 0)
                return (true, null, null);

            var resultado = await ImgbbHelper.SubirImagenAsync(imagenFile, _configuration["Imgbb:ApiKey"]);
            if (!resultado.Exitoso)
                return (false, null, resultado.Error ?? "No se pudo subir la imagen a ImgBB.");

            return (true, resultado, null);
        }

        private IActionResult? ValidarAcceso()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetString("Rol") != "ADMINISTRADOR")
                return StatusCode(403, "Solo el administrador puede acceder a esta sección.");

            return null;
        }

        public async Task<IActionResult> Index(string? pestana)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var productos = await _apiService.GetAllAsync();
            var model = productos.ToModel();

            // La API ya devuelve cada promoción con sus productos incluidos y
            // con el ahorro ya calculado, así que el listado se mapea directo.
            var promociones = await _promocionesApiService.GetAllAsync();
            ViewBag.Promociones = promociones?.Select(ToListadoViewModel).ToList()
                ?? new List<PromocionListadoViewModel>();
            ViewBag.TotalPromociones = ViewBag.Promociones.Count;
            ViewBag.TotalProductos = model.Count;
            ViewBag.PestanaActiva = pestana ?? "productos";

            return View("~/Views/Administrador/Productos/Index.cshtml", model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var producto = await _apiService.GetByIdAsync(id.Value);
            if (producto == null) return NotFound();

            var model = producto.ToModel();
            return View("~/Views/Administrador/Productos/Details.cshtml", model);
        }

        public async Task<IActionResult> Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var categorias = await _categoriasApiService.GetAllAsync();
            ViewBag.IdCategoria = new SelectList(categorias, "IdCategoria", "Nombre");

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

            var model = new ProductoRecetaViewModel
            {
                Producto = new Producto(),
                Receta = new Receta(),
                IncluirReceta = false
            };

            return View("~/Views/Administrador/Productos/Create.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoRecetaViewModel model, IFormFile? imagenFile)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (ModelState.IsValid)
            {
                var producto = new ProductoDto
                {
                    IdCategoria = model.Producto.IdCategoria,
                    Codigo = model.Producto.Codigo,
                    Nombre = model.Producto.Nombre,
                    Descripcion = model.Producto.Descripcion,
                    Precio = model.Producto.Precio,
                    Activo = model.Producto.Activo,
                    ImagenUrl = model.Producto.ImagenUrl,
                    DisplayUrl = model.Producto.DisplayUrl
                };

                // Subir la imagen (si viene) antes de crear el producto en la API.
                var (okImagen, resultado, errorImagen) = await SubirImagenSiViene(imagenFile);
                if (!okImagen)
                {
                    ModelState.AddModelError(string.Empty, errorImagen!);
                }
                else
                {
                    if (resultado != null)
                    {
                        producto.ImagenUrl = resultado.Url;
                        producto.DisplayUrl = resultado.DisplayUrl;
                    }

                    var (success, message) = await _apiService.CreateAsync(producto);
                    if (success)
                        return RedirectToAction(nameof(Index));

                    ModelState.AddModelError(string.Empty, message);
                }
            }

            var categorias = await _categoriasApiService.GetAllAsync();
            ViewBag.IdCategoria = new SelectList(categorias, "IdCategoria", "Nombre");

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

            return View("~/Views/Administrador/Productos/Create.cshtml", model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var producto = await _apiService.GetByIdAsync(id.Value);
            if (producto == null) return NotFound();

            var categorias = await _categoriasApiService.GetAllAsync();
            ViewBag.IdCategoria = new SelectList(categorias, "IdCategoria", "Nombre");

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

            var model = producto.ToRecetaViewModel();
            return View("~/Views/Administrador/Productos/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductoRecetaViewModel model, IFormFile? imagenFile)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id != model.Producto.IdProducto) return NotFound();

            if (ModelState.IsValid)
            {
                var producto = new ProductoDto
                {
                    IdProducto = model.Producto.IdProducto,
                    IdCategoria = model.Producto.IdCategoria,
                    Codigo = model.Producto.Codigo,
                    Nombre = model.Producto.Nombre,
                    Descripcion = model.Producto.Descripcion,
                    Precio = model.Producto.Precio,
                    Activo = model.Producto.Activo,
                    ImagenUrl = model.Producto.ImagenUrl,
                    DisplayUrl = model.Producto.DisplayUrl
                };

                var (okImagen, resultado, errorImagen) = await SubirImagenSiViene(imagenFile);
                if (!okImagen)
                {
                    ModelState.AddModelError(string.Empty, errorImagen!);
                }
                else
                {
                    if (resultado != null)
                    {
                        producto.ImagenUrl = resultado.Url;
                        producto.DisplayUrl = resultado.DisplayUrl;
                    }
                    else
                    {
                        // Sin archivo nuevo: conservar la imagen actual. Si no se
                        // hace, la edición enviaría la URL en blanco y la API
                        // borraría la imagen del producto.
                        var actual = await _apiService.GetByIdAsync(id);
                        if (actual != null)
                        {
                            producto.ImagenUrl = actual.ImagenUrl;
                            producto.DisplayUrl = actual.DisplayUrl;
                        }
                    }

                    var (success, message) = await _apiService.EditAsync(id, producto);
                    if (success)
                        return RedirectToAction(nameof(Index));

                    ModelState.AddModelError(string.Empty, message);
                }
            }

            var categorias = await _categoriasApiService.GetAllAsync();
            ViewBag.IdCategoria = new SelectList(categorias, "IdCategoria", "Nombre");

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

            return View("~/Views/Administrador/Productos/Edit.cshtml", model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var producto = await _apiService.GetByIdAsync(id.Value);
            if (producto == null) return NotFound();

            var model = producto.ToModel();
            return View("~/Views/Administrador/Productos/Delete.cshtml", model);
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

        // Quita solo la imagen del producto, sin tocar el resto de sus datos.
        // Lo usa el botón 'Eliminar imagen' del formulario de edición.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarImagen(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var (success, message) = await _apiService.QuitarImagenAsync(id);
            if (!success)
                TempData["Error"] = message;
            else
                TempData["Mensaje"] = "Imagen del producto eliminada.";

            return RedirectToAction(nameof(Edit), new { id });
        }
    }

    public partial class ProductosController : Controller
    {
        // =============================================================
        //  PROMOCIONES
        //  La API es la única que habla con la base de datos. El listado
        //  ya llega con los productos incluidos y con el ahorro calculado,
        //  así que estas acciones solo mapean y delegan.
        // =============================================================

        // DTO de la API -> fila del listado.
        private static PromocionListadoViewModel ToListadoViewModel(PromocionDto dto)
        {
            return new PromocionListadoViewModel
            {
                IdPromocion = dto.IdPromocion,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Tipo = dto.Tipo,
                FechaInicio = dto.FechaInicio.ToDateTime(TimeOnly.MinValue),
                FechaFin = dto.FechaFin.ToDateTime(TimeOnly.MinValue),
                Estado = dto.Estado,
                EstaActiva = dto.EstaActiva,
                TieneAhorro = dto.TieneAhorro,
                PorcentajeAhorro = dto.PorcentajeAhorro,
                Ahorro = dto.Ahorro,
                CantidadProductos = dto.CantidadProductos,
                ResumenProductos = dto.ResumenProductos,
                SumaProductos = dto.SumaProductos,
                Precio = dto.Valor,
                ImagenUrl = dto.ImagenUrl,
                Incluidos = dto.Incluidos.Select(i => new PromocionListadoItem
                {
                    IdProducto = i.IdProducto,
                    ProductoNombre = i.ProductoNombre,
                    ProductoPrecio = i.ProductoPrecio,
                    ProductoActivo = i.ProductoActivo,
                    Cantidad = i.Cantidad,
                    Subtotal = i.Subtotal
                }).ToList()
            };
        }

        // DTO de la API -> detalle.
        private static PromocionDetalleViewModel ToDetalleViewModel(PromocionDto dto)
        {
            return new PromocionDetalleViewModel
            {
                IdPromocion = dto.IdPromocion,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Tipo = dto.Tipo,
                FechaInicio = dto.FechaInicio.ToDateTime(TimeOnly.MinValue),
                FechaFin = dto.FechaFin.ToDateTime(TimeOnly.MinValue),
                Estado = dto.Estado,
                EstaActiva = dto.EstaActiva,
                SumaProductos = dto.SumaProductos,
                Ahorro = dto.Ahorro,
                TieneAhorro = dto.TieneAhorro,
                PorcentajeAhorro = dto.PorcentajeAhorro,
                Precio = dto.Valor,
                ImagenUrl = dto.ImagenUrl,
                CantidadProductos = dto.CantidadProductos,
                ResumenProductos = dto.ResumenProductos,
                Incluidos = dto.Incluidos.Select(i => new Producto
                {
                    IdProducto = i.IdProducto,
                    Nombre = i.ProductoNombre ?? "(producto eliminado)",
                    Precio = i.ProductoPrecio,
                    Activo = i.ProductoActivo,
                    Cantidad = i.Cantidad
                }).ToList()
            };
        }

        // El formulario pinta el catálogo completo con checkbox, así que se cruza
        // el catálogo con lo que la API dice que ya está incluido.
        private static List<Producto> ArmarCatalogoParaFormulario(
            IEnumerable<ProductoDto>? catalogo,
            IEnumerable<PromocionProductoDto>? incluidos)
        {
            var mapa = (incluidos ?? Enumerable.Empty<PromocionProductoDto>())
                .ToDictionary(i => i.IdProducto);

            return (catalogo ?? Enumerable.Empty<ProductoDto>())
                .Select(p => new Producto
                {
                    IdProducto = p.IdProducto,
                    Codigo = p.Codigo,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Activo = p.Activo,
                    ImagenUrl = p.ImagenUrl,
                    DisplayUrl = p.DisplayUrl,
                    IdCategoria = p.IdCategoria,
                    CategoriaNombre = p.CategoriaNombre,
                    TieneReceta = p.TieneReceta,
                    Seleccionado = mapa.ContainsKey(p.IdProducto),
                    Cantidad = mapa.TryGetValue(p.IdProducto, out var inc) && inc.Cantidad > 0
                        ? inc.Cantidad
                        : 1
                })
                .ToList();
        }

        // Al volver del POST el catálogo se reconstruye igual, pero respetando lo
        // que el usuario marcó. El checkbox desmarcado no viaja en el form, así
        // que se toma elposted por IdProducto y se cruza con el catálogo.
        private static List<Producto> ArmarCatalogoConMarcas(
            IEnumerable<ProductoDto>? catalogo,
            List<Producto>? posted)
        {
            var marcadas = (posted ?? new List<Producto>())
                .Where(p => p.Seleccionado)
                .ToDictionary(p => p.IdProducto, p => p.Cantidad);

            var baseCatalogo = ArmarCatalogoParaFormulario(catalogo, null);

            return baseCatalogo
                .Select(p =>
                {
                    if (marcadas.ContainsKey(p.IdProducto))
                    {
                        p.Seleccionado = true;
                        if (marcadas[p.IdProducto] > 0)
                            p.Cantidad = marcadas[p.IdProducto];
                    }
                    return p;
                })
                .ToList();
        }

        // ViewModel del formulario -> request de escritura de la API.
        private static PromocionRequestDto ToRequestDto(PromocionFormViewModel model)
        {
            var hoy = DateTime.Today;

            return new PromocionRequestDto
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                Tipo = string.IsNullOrWhiteSpace(model.Tipo) ? "PAQUETE" : model.Tipo,
                Valor = model.Precio,
                FechaInicio = DateOnly.FromDateTime(model.FechaInicio ?? hoy),
                FechaFin = DateOnly.FromDateTime(model.FechaFin ?? hoy.AddYears(1)),
                Estado = model.Estado == "ACTIVA" ? "ACTIVA" : "INACTIVA",
                ImagenUrl = model.ImagenActual,
                DisplayUrl = model.DisplayUrlActual,
                // Solo se envían los productos marcados; la API borra los
                // anteriores y vuelve a crear estos.
                DetallePromociones = model.Incluidos
                    .Where(p => p.Seleccionado)
                    .Select(p => new DetallePromocionRequestDto
                    {
                        IdProducto = p.IdProducto,
                        Cantidad = p.Cantidad > 0 ? p.Cantidad : 1
                    })
                    .ToList()
            };
        }

        public async Task<IActionResult> PromocionDetails(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var promocion = await _promocionesApiService.GetByIdAsync(id.Value);
            if (promocion == null) return NotFound();

            return View("~/Views/Administrador/Productos/PromocionDetails.cshtml",
                ToDetalleViewModel(promocion));
        }

        public async Task<IActionResult> PromocionCreate()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var productos = await _apiService.GetAllAsync();

            var model = new PromocionFormViewModel
            {
                FechaInicio = DateTime.Today,
                FechaFin = DateTime.Today.AddMonths(1),
                Estado = "ACTIVA",
                Incluidos = ArmarCatalogoParaFormulario(productos, null)
            };

            return View("~/Views/Administrador/Productos/PromocionForm.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromocionCreate(PromocionFormViewModel model, IFormFile? imagenFile)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var productos = await _apiService.GetAllAsync();

            if (string.IsNullOrWhiteSpace(model.Nombre))
                ModelState.AddModelError(nameof(model.Nombre), "El nombre es obligatorio.");

            if (!ModelState.IsValid)
            {
                model.Incluidos = ArmarCatalogoConMarcas(productos, model.Incluidos);
                model.EsEdicion = false;
                return View("~/Views/Administrador/Productos/PromocionForm.cshtml", model);
            }

            // Subir la imagen (si viene) antes de crear la promoción en la API.
            var (okImagen, resultado, errorImagen) = await SubirImagenSiViene(imagenFile);
            if (!okImagen)
            {
                ModelState.AddModelError(string.Empty, errorImagen!);
                model.Incluidos = ArmarCatalogoConMarcas(productos, model.Incluidos);
                model.EsEdicion = false;
                return View("~/Views/Administrador/Productos/PromocionForm.cshtml", model);
            }

            if (resultado != null)
            {
                model.ImagenActual = resultado.Url;
                model.DisplayUrlActual = resultado.DisplayUrl;
            }

            var (success, message) = await _promocionesApiService.CreateAsync(ToRequestDto(model));
            if (success)
            {
                TempData["Ok"] = "Promoción creada correctamente.";
                return RedirectToAction(nameof(Index), new { pestana = "promociones" });
            }

            ModelState.AddModelError(string.Empty, message);
            model.Incluidos = ArmarCatalogoConMarcas(productos, model.Incluidos);
            model.EsEdicion = false;
            return View("~/Views/Administrador/Productos/PromocionForm.cshtml", model);
        }

        public async Task<IActionResult> PromocionEdit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var promocion = await _promocionesApiService.GetByIdAsync(id.Value);
            if (promocion == null) return NotFound();

            var productos = await _apiService.GetAllAsync();

            var model = new PromocionFormViewModel
            {
                IdPromocion = promocion.IdPromocion,
                Nombre = promocion.Nombre,
                Descripcion = promocion.Descripcion,
                Tipo = promocion.Tipo,
                FechaInicio = promocion.FechaInicio.ToDateTime(TimeOnly.MinValue),
                FechaFin = promocion.FechaFin.ToDateTime(TimeOnly.MinValue),
                Estado = promocion.Estado,
                EsEdicion = true,
                Precio = promocion.Valor,
                SumaProductos = promocion.SumaProductos,
                Ahorro = promocion.Ahorro,
                PorcentajeAhorro = promocion.PorcentajeAhorro,
                ImagenActual = promocion.ImagenUrl,
                DisplayUrlActual = promocion.DisplayUrl,
                Incluidos = ArmarCatalogoParaFormulario(productos, promocion.Incluidos)
            };

            return View("~/Views/Administrador/Productos/PromocionForm.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromocionEdit(int id, PromocionFormViewModel model, IFormFile? imagenFile)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var originales = await _promocionesApiService.GetByIdAsync(id);
            if (originales == null) return NotFound();

            var productos = await _apiService.GetAllAsync();

            // El checkbox desmarcado no manda valor, así que el binder deja
            // Seleccionado=false en todos. Se reconstruye el catálogo cruzando
            // lo posted con la lista real de productos.
            model.Incluidos = ArmarCatalogoConMarcas(productos, model.Incluidos);
            model.IdPromocion = id;
            model.EsEdicion = true;
            model.ImagenActual = originales.ImagenUrl;
            model.DisplayUrlActual = originales.DisplayUrl;

            // Si viene un archivo nuevo se sube a ImgBB y se reemplaza la imagen
            // actual; si no, se conserva la que ya tiene la promoción.
            var (okImagen, resultado, errorImagen) = await SubirImagenSiViene(imagenFile);
            if (!okImagen)
                ModelState.AddModelError(string.Empty, errorImagen!);
            else if (resultado != null)
            {
                model.ImagenActual = resultado.Url;
                model.DisplayUrlActual = resultado.DisplayUrl;
            }

            if (string.IsNullOrWhiteSpace(model.Nombre))
                ModelState.AddModelError(nameof(model.Nombre), "El nombre es obligatorio.");

            if (!ModelState.IsValid)
            {
                model.SumaProductos = originales.SumaProductos;
                return View("~/Views/Administrador/Productos/PromocionForm.cshtml", model);
            }

            var (success, message) = await _promocionesApiService.EditAsync(id, ToRequestDto(model));
            if (success)
            {
                TempData["Ok"] = "Promoción actualizada correctamente.";
                return RedirectToAction(nameof(Index), new { pestana = "promociones" });
            }

            ModelState.AddModelError(string.Empty, message);
            model.SumaProductos = originales.SumaProductos;
            return View("~/Views/Administrador/Productos/PromocionForm.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromocionEliminarImagen(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var (success, message) = await _promocionesApiService.QuitarImagenAsync(id);
            if (!success)
                TempData["Error"] = message;

            return RedirectToAction(nameof(PromocionEdit), new { id });
        }

        public async Task<IActionResult> PromocionDelete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null) return NotFound();

            var promocion = await _promocionesApiService.GetByIdAsync(id.Value);
            if (promocion == null) return NotFound();

            return View("~/Views/Administrador/Productos/PromocionDelete.cshtml",
                ToDetalleViewModel(promocion));
        }

        [HttpPost, ActionName("PromocionDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromocionDeleteConfirmed(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var (success, message) = await _promocionesApiService.DeleteAsync(id);
            if (success)
                TempData["Ok"] = "Promoción eliminada correctamente.";
            else
                TempData["Error"] = message;

            return RedirectToAction(nameof(Index), new { pestana = "promociones" });
        }
    }
}
