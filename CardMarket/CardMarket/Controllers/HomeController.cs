using CardMarket.Data;
using CardMarket.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CardMarket.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MvcTiendaContexto _context;

        public HomeController(ILogger<HomeController> logger, MvcTiendaContexto context)
        {
            _logger = logger;
            _context = context;
        }

        
        public IActionResult Index()
        {
            // Busca el empleado correspondiente al usuario actual. Si existe, activa la
            // vista (View) y en caso contrario, se redirige para crear el empleado.
            string? emailCliente = User.Identity.Name;
            Cliente? cliente = _context.Clientes.Where(c => c.Email == emailCliente)
            .FirstOrDefault();
            if (User.Identity.IsAuthenticated &&
            User.IsInRole("Usuario") &&
            cliente == null)
            {
                return RedirectToAction("Create", "MisDatos");
            }
            return View();

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