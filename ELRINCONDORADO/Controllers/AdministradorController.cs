using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Data;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Controllers
{
    public class AdministradorController : Controller
    {
        private readonly AppDbContext _context;

        public AdministradorController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (string.IsNullOrEmpty(rol) || !rol.Equals("ADMINISTRADOR", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Login", "Auth");

            return View(await ConstruirResumen());
        }

        // Insumos que están por debajo del stock mínimo y cuánto se lleva facturado en el turno
        // (desde el último cierre de caja; al cerrar caja vuelve a cero).
        private async Task<AdminResumenViewModel> ConstruirResumen()
        {
            var resumen = new AdminResumenViewModel();

            resumen.InsumosBajoMinimo = await _context.Insumos
                .Where(i => i.Activo && i.StockMinimo > 0 && i.StockActual < i.StockMinimo)
                .OrderBy(i => i.StockActual - i.StockMinimo)
                .ThenBy(i => i.Nombre)
                .Select(i => new InsumoBajoMinimoViewModel
                {
                    IdInsumo = i.IdInsumo,
                    Nombre = i.Nombre,
                    UnidadMedida = i.UnidadMedida,
                    StockActual = i.StockActual,
                    StockMinimo = i.StockMinimo
                })
                .ToListAsync();

            // Ventas cobradas (facturadas) que no fueron canceladas por el administrador
            var ventas = await _context.Pedidos
                .Where(p => p.EstadoPago != null && p.EstadoPago != "" && p.EstadoPago != "PENDIENTE"
                            && p.Estado != "CANCELADO")
                .Select(p => new { p.FechaCreacion, p.Total })
                .ToListAsync();

            // El turno arranca después del último cierre de caja: al cerrar, el monto se reinicia
            var ultimoCierre = await _context.CierresCaja
                .OrderByDescending(c => c.IdCierre)
                .FirstOrDefaultAsync();

            resumen.UltimoCierre = ultimoCierre?.Fecha;

            var delTurno = ultimoCierre == null
                ? ventas
                : ventas.Where(v => v.FechaCreacion > ultimoCierre.Fecha).ToList();

            resumen.VentasTurno = delTurno.Count;
            resumen.VendidoTurno = delTurno.Sum(v => v.Total);

            var hoy = DateTime.Today;
            var delDia = ventas.Where(v => v.FechaCreacion.ToLocalTime().Date == hoy).ToList();
            resumen.VentasHoy = delDia.Count;
            resumen.VendidoHoy = delDia.Sum(v => v.Total);

            return resumen;
        }
    }
}
