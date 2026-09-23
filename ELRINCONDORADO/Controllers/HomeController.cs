using ELRINCONDORADO.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ELRINCONDORADO.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            // Verificar si el usuario está autenticado (verificar sesión)
            var usuario = HttpContext.Session.GetString("Usuario");
            if (string.IsNullOrEmpty(usuario))
            {
                return RedirectToAction("Login", "Auth");
            }
            // Redirigir al panel según el rol
            var rol = HttpContext.Session.GetString("Rol");
            return (rol?.ToUpper()) switch
            {
                "ADMINISTRADOR" => RedirectToAction("Index", "Administrador"),
                "CAJERO" => RedirectToAction("Index", "Cajero"),
                "MESERO" => RedirectToAction("Index", "Mesero"),
                "COCINERO" => RedirectToAction("Index", "Cocina"),
                "ALMACEN" => RedirectToAction("Index", "Almacen"),
                _ => RedirectToAction("Login", "Auth")
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
