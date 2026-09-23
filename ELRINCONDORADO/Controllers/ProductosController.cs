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

        // GET: Productos
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var productos = _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Receta)
                .OrderByDescending(p => p.IdProducto);
            return View("~/Views/Administrador/Productos/Index.cshtml", await productos.ToListAsync());
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

            return View("~/Views/Administrador/Productos/Details.cshtml", producto);
        }

        // GET: Productos/Create
        public IActionResult Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            CargarListas();
            return View("~/Views/Administrador/Productos/Create.cshtml", new ProductoRecetaViewModel());
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
                    return View("~/Views/Administrador/Productos/Create.cshtml", modelo);
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
                        return View("~/Views/Administrador/Productos/Create.cshtml", modelo);
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
            return View("~/Views/Administrador/Productos/Create.cshtml", modelo);
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
            return View("~/Views/Administrador/Productos/Edit.cshtml", modelo);
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
                    return View("~/Views/Administrador/Productos/Edit.cshtml", modelo);
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
                        return View("~/Views/Administrador/Productos/Edit.cshtml", modelo);
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
            return View("~/Views/Administrador/Productos/Edit.cshtml", modelo);
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

            return View("~/Views/Administrador/Productos/Delete.cshtml", producto);
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