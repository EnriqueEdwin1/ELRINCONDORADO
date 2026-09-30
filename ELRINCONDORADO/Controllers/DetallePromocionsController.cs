using Microsoft.AspNetCore.Mvc;

namespace ELRINCONDORADO.Controllers
{
    // Alias del listado de promociones, igual que PromocionesController.
    // El detalle de una promoción con sus productos se resuelve en
    // ProductosController.PromocionDetails.
    public class DetallePromocionsController : Controller
    {
        public IActionResult Index() => RedirectToAction("Index", "Productos", new { pestana = "promociones" });

        public IActionResult Details(int? id) => RedirectToAction("PromocionDetails", "Productos", new { id });
    }
}
