using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class InsumosController : Controller
    {
        private readonly AppDbContext _context;

        public InsumosController(AppDbContext context)
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

        // GET: Insumos
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            return View("~/Views/Administrador/Insumos/Index.cshtml", await _context.Insumos.Include(i => i.Destino).ToListAsync());
        }

        // GET: Insumos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var insumo = await _context.Insumos
                .Include(i => i.Destino)
                .FirstOrDefaultAsync(m => m.IdInsumo == id);
            if (insumo == null)
            {
                return NotFound();
            }

            return View("~/Views/Administrador/Insumos/Details.cshtml", insumo);
        }

        // GET: Insumos/Create
        public async Task<IActionResult> Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            await CargarDestinosAsync();
            return View("~/Views/Administrador/Insumos/Create.cshtml", new Insumo());
        }

        // POST: Insumos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdInsumo,Nombre,Descripcion,UnidadMedida,StockActual,StockMinimo,CostoUnitario,Activo,IdDestino")] Insumo insumo)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            await CargarDestinosAsync();
            NormalizarNumericos(insumo);

            if (ModelState.IsValid)
            {
                if (await _context.Insumos.AnyAsync(i => i.Nombre == insumo.Nombre))
                {
                    ModelState.AddModelError(string.Empty, "Ese nombre de insumo ya existe.");
                    return View("~/Views/Administrador/Insumos/Create.cshtml", insumo);
                }

                _context.Add(insumo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Administrador/Insumos/Create.cshtml", insumo);
        }

        // GET: Insumos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var insumo = await _context.Insumos.FindAsync(id);
            if (insumo == null)
            {
                return NotFound();
            }
            await CargarDestinosAsync();
            return View("~/Views/Administrador/Insumos/Edit.cshtml", insumo);
        }

        // POST: Insumos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdInsumo,Nombre,Descripcion,UnidadMedida,StockActual,StockMinimo,CostoUnitario,Activo,IdDestino")] Insumo insumo)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id != insumo.IdInsumo)
            {
                return NotFound();
            }

            await CargarDestinosAsync();
            NormalizarNumericos(insumo);

            if (ModelState.IsValid)
            {
                if (await _context.Insumos.AnyAsync(i => i.Nombre == insumo.Nombre && i.IdInsumo != insumo.IdInsumo))
                {
                    ModelState.AddModelError(string.Empty, "Ese nombre de insumo ya existe.");
                    return View("~/Views/Administrador/Insumos/Edit.cshtml", insumo);
                }

                try
                {
                    // La modificación de un insumo sí queda registrada en Movimientos (las compras
                    // no: esas se ven en su propia pantalla). Se compara el stock y el costo
                    // antes de guardar para poder dejar el detalle del ajuste.
                    var anterior = await _context.Insumos.AsNoTracking()
                        .FirstOrDefaultAsync(i => i.IdInsumo == insumo.IdInsumo);

                    _context.Update(insumo);
                    await _context.SaveChangesAsync();

                    if (anterior != null)
                    {
                        await RegistrarAjuste(anterior, insumo);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InsumoExists(insumo.IdInsumo))
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
            return View("~/Views/Administrador/Insumos/Edit.cshtml", insumo);
        }

        // GET: Insumos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var insumo = await _context.Insumos
                .FirstOrDefaultAsync(m => m.IdInsumo == id);
            if (insumo == null)
            {
                return NotFound();
            }

            return View("~/Views/Administrador/Insumos/Delete.cshtml", insumo);
        }

        // POST: Insumos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var insumo = await _context.Insumos.FindAsync(id);
            if (insumo != null)
            {
                _context.Insumos.Remove(insumo);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool InsumoExists(int id)
        {
            return _context.Insumos.Any(e => e.IdInsumo == id);
        }

        /// <summary>
        /// Deja rastro en Movimientos cuando se edita un insumo. Solo se genera un registro si
        /// cambian el stock o el costo; la cantidad es la diferencia de stock con signo
        /// (+ suma, - resta) y el motivo detalla qué se modificó.
        /// Las compras NO pasan por aquí: se registran en su propia pantalla.
        /// </summary>
        private async Task RegistrarAjuste(Insumo anterior, Insumo nuevo)
        {
            var diferencia = nuevo.StockActual - anterior.StockActual;
            var cambioCosto = nuevo.CostoUnitario != anterior.CostoUnitario;

            if (diferencia == 0 && !cambioCosto) return;

            // El movimiento necesita un empleado válido (llave foránea); si la sesión no lo trae,
            // se omite el registro antes que romper la edición del insumo.
            var idSesion = HttpContext.Session.GetString("UsuarioId");
            if (!int.TryParse(idSesion, out var idEmpleado) ||
                !await _context.Empleados.AnyAsync(e => e.IdEmpleado == idEmpleado))
            {
                return;
            }

            var partes = new List<string>();

            if (diferencia != 0)
            {
                partes.Add($"stock {anterior.StockActual:0.###} → {nuevo.StockActual:0.###}"
                    + $" ({(diferencia > 0 ? "+" : "")}{diferencia:0.###} {nuevo.UnidadMedida})");
            }
            if (cambioCosto)
            {
                partes.Add($"costo {anterior.CostoUnitario:0.##} → {nuevo.CostoUnitario:0.##}");
            }

            _context.MovimientosInventario.Add(new MovimientoInventario
            {
                IdInsumo = nuevo.IdInsumo,
                IdEmpleado = idEmpleado,
                TipoMovimiento = "AJUSTE",
                Cantidad = diferencia,
                Fecha = DateTime.Now,
                Motivo = "Modificación de insumo: " + string.Join(" · ", partes)
            });
            await _context.SaveChangesAsync();
        }

        // Carga en ViewBag los destinos disponibles para el select de los formularios
        private async Task CargarDestinosAsync()
        {
            ViewBag.Destinos = new SelectList(
                await _context.DestinosInsumos.OrderBy(d => d.Nombre).ToListAsync(),
                "IdDestino", "Nombre");
        }

        // Convierte en 0 los campos numéricos que lleguen vacíos, evitando el error de binding "The value '' is invalid"
        private void NormalizarNumericos(Insumo insumo)
        {
            var campos = new[] { "StockActual", "StockMinimo", "CostoUnitario" };
            foreach (var campo in campos)
            {
                var valorCrudo = Convert.ToString(HttpContext.Request.Form[campo]);
                var entry = ModelState[campo];
                if (entry != null && entry.Errors.Count > 0 && string.IsNullOrWhiteSpace(valorCrudo))
                {
                    entry.Errors.Clear();
                }
            }
        }
    }
}
