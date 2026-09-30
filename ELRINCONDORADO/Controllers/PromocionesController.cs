using Microsoft.AspNetCore.Mvc;

namespace ELRINCONDORADO.Controllers
{
    // El CRUD de promociones vive en ProductosController, dentro de la pestaña
    // 'promociones' de /Productos. Este controlador queda solo como alias para
    // que los enlaces antiguos (/Promociones/...) sigan funcionando.
    //
    // Antes duplicaba la lógica y apuntaba a Views/Administrador/Promociones/,
    // una carpeta que no existe: por eso esas rutas lanzaban error.
    public class PromocionesController : Controller
    {
        public IActionResult Index() => RedirectToAction("Index", "Productos", new { pestana = "promociones" });

        public IActionResult Details(int? id) => RedirectToAction("PromocionDetails", "Productos", new { id });

        public IActionResult Create() => RedirectToAction("PromocionCreate", "Productos");

        public IActionResult Edit(int? id) => RedirectToAction("PromocionEdit", "Productos", new { id });

        public IActionResult Delete(int? id) => RedirectToAction("PromocionDelete", "Productos", new { id });
    }
}
