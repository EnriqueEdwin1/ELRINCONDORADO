using Microsoft.AspNetCore.Mvc;

namespace ELRINCONDORADO.Controllers
{
    public class AdministradorController : Controller
    {
        public IActionResult Index()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (string.IsNullOrEmpty(rol) || !rol.Equals("ADMINISTRADOR", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Login", "Auth");

            return View();
        }
    }
}