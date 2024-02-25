using cm_web2.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;
using System.Diagnostics;

namespace cm_web2.Controllers
{
    public class HomeController : Controller
    {
        const string connectionString = "Data Source=data.db;Version=3;";
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult SessionError()
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