using Microsoft.AspNetCore.Mvc;

namespace ELRINCONDORADO.Controllers
{
    public class AlmacenController : Controller
    {
        public IActionResult Index()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (string.IsNullOrEmpty(rol) || !rol.Equals("ALMACEN", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Login", "Auth");

            return View();
        }
    }
}