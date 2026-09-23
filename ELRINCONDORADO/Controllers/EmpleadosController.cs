using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;
using System.Security.Cryptography;
using System.Text;

namespace ELRINCONDORADO.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly AppDbContext _context;

        public EmpleadosController(AppDbContext context)
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

        // GET: Empleados
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var empleados = _context.Empleados.Include(e => e.Rol);
            return View("~/Views/Administrador/Empleados/Index.cshtml", await empleados.ToListAsync());
        }

        // GET: Empleados/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .Include(e => e.Rol)
                .FirstOrDefaultAsync(m => m.IdEmpleado == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View("~/Views/Administrador/Empleados/Details.cshtml", empleado);
        }

        // GET: Empleados/Create
        public IActionResult Create()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            CargarRoles();
            return View("~/Views/Administrador/Empleados/Create.cshtml");
        }

        // POST: Empleados/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdEmpleado,IdRol,Nombre,Apellido,Usuario,PasswordHash,Telefono,FechaContratacion,Estado")] Empleado empleado)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (ModelState.IsValid)
            {
                var rolNombre = await _context.Roles
                    .Where(r => r.IdRol == empleado.IdRol)
                    .Select(r => r.Nombre)
                    .FirstOrDefaultAsync();

                if (rolNombre == "ADMINISTRADOR")
                {
                    ModelState.AddModelError(string.Empty, "Un administrador no puede crear otro usuario con rol administrador.");
                    CargarRoles(empleado.IdRol);
                    return View("~/Views/Administrador/Empleados/Create.cshtml", empleado);
                }

                if (await _context.Empleados.AnyAsync(e => e.Usuario == empleado.Usuario))
                {
                    ModelState.AddModelError(string.Empty, "Ese nombre de usuario ya está en uso.");
                    CargarRoles(empleado.IdRol);
                    return View("~/Views/Administrador/Empleados/Create.cshtml", empleado);
                }

                if (!string.IsNullOrWhiteSpace(empleado.PasswordHash))
                    empleado.PasswordHash = HashPassword(empleado.PasswordHash);

                _context.Add(empleado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            CargarRoles(empleado.IdRol);
            return View("~/Views/Administrador/Empleados/Create.cshtml", empleado);
        }

        // GET: Empleados/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .Include(e => e.Rol)
                .FirstOrDefaultAsync(e => e.IdEmpleado == id);
            if (empleado == null)
            {
                return NotFound();
            }
            CargarRoles(empleado.IdRol);
            ViewBag.EsAdministrador = empleado.Rol?.Nombre == "ADMINISTRADOR";
            return View("~/Views/Administrador/Empleados/Edit.cshtml", empleado);
        }

        // POST: Empleados/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdEmpleado,IdRol,Nombre,Apellido,Usuario,PasswordHash,Telefono,FechaContratacion,Estado")] Empleado empleado, string? nuevaPassword)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id != empleado.IdEmpleado)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await _context.Empleados.AnyAsync(e => e.Usuario == empleado.Usuario && e.IdEmpleado != empleado.IdEmpleado))
                {
                    ModelState.AddModelError(string.Empty, "Ese nombre de usuario ya está en uso.");
                    CargarRoles(empleado.IdRol);
                    return View("~/Views/Administrador/Empleados/Edit.cshtml", empleado);
                }

                var original = await _context.Empleados
                    .Include(e => e.Rol)
                    .FirstOrDefaultAsync(e => e.IdEmpleado == id);
                if (original == null)
                {
                    return NotFound();
                }

                var esOriginalAdmin = original.Rol?.Nombre == "ADMINISTRADOR";
                var nuevoRolEsAdmin = await _context.Roles
                    .AnyAsync(r => r.IdRol == empleado.IdRol && r.Nombre == "ADMINISTRADOR");

                if (nuevoRolEsAdmin && !esOriginalAdmin)
                {
                    ModelState.AddModelError(string.Empty, "Un administrador no puede otorgar el rol administrador a otro empleado.");
                    CargarRoles(empleado.IdRol);
                    return View("~/Views/Administrador/Empleados/Edit.cshtml", empleado);
                }

                if (esOriginalAdmin)
                {
                    // A un administrador solo se le permite modificar su teléfono
                    original.Telefono = empleado.Telefono;
                }
                else
                {
                    original.IdRol = empleado.IdRol;
                    original.Nombre = empleado.Nombre;
                    original.Apellido = empleado.Apellido;
                    original.Usuario = empleado.Usuario;
                    original.Telefono = empleado.Telefono;
                    original.FechaContratacion = empleado.FechaContratacion;
                    original.Estado = empleado.Estado;
                }

                if (!string.IsNullOrWhiteSpace(nuevaPassword))
                    original.PasswordHash = HashPassword(nuevaPassword);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpleadoExists(empleado.IdEmpleado))
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
            CargarRoles(empleado.IdRol);
            return View("~/Views/Administrador/Empleados/Edit.cshtml", empleado);
        }

        // GET: Empleados/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .Include(e => e.Rol)
                .FirstOrDefaultAsync(m => m.IdEmpleado == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View("~/Views/Administrador/Empleados/Delete.cshtml", empleado);
        }

        // POST: Empleados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var empleado = await _context.Empleados
                .Include(e => e.Rol)
                .FirstOrDefaultAsync(e => e.IdEmpleado == id);
            if (empleado != null)
            {
                if (empleado.Rol?.Nombre == "ADMINISTRADOR")
                {
                    TempData["Error"] = "No puedes eliminar a un usuario administrador.";
                    return RedirectToAction(nameof(Index));
                }

                var tieneRegistros = await _context.Pedidos.AnyAsync(p => p.IdEmpleado == id)
                    || await _context.Ventas.AnyAsync(v => v.IdEmpleado == id)
                    || await _context.Compras.AnyAsync(c => c.IdEmpleado == id)
                    || await _context.MovimientosInventario.AnyAsync(m => m.IdEmpleado == id);

                if (tieneRegistros)
                {
                    TempData["Error"] = "No se puede eliminar el empleado porque tiene ventas, pedidos, compras o movimientos asociados.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Empleados.Remove(empleado);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoExists(int id)
        {
            return _context.Empleados.Any(e => e.IdEmpleado == id);
        }

        // Carga el listado de roles excluyendo al administrador (no se puede crear/grantar ese rol)
        private void CargarRoles(int? idRol = null)
        {
            var roles = _context.Roles
                .Where(r => r.Nombre != "ADMINISTRADOR")
                .OrderBy(r => r.Nombre)
                .ToList();
            ViewBag.IdRol = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(roles, "IdRol", "Nombre", idRol);
        }

        private static string HashPassword(string password)
        {
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        }
    }
}
