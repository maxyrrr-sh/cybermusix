using cm;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using cm_web2.Models;
using System.Data.SQLite;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;

namespace cm_web2.Controllers
{
    public class AccountController : Controller
    {
        const string connectionString = "Data Source=data.db;Version=3;";

        static User user = null;

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                user = new User(model.Username, model.Password);
                user.Register();
                return RedirectToAction("Login");
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Logout()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Logout(RegisterViewModel model)
        {
            SessionManager.sessionToken = null;
            user = null;
            Console.WriteLine("logged out");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Виклик методу Login з класу User
            user = new User(model.Username, model.Password);
            if (user.Login())
            {
                // Генерація токену сесії
                SessionManager.sessionToken = SessionManager.GenerateToken(user.getUsername());
                Console.WriteLine("Session has been started with token " + SessionManager.sessionToken);
                // Збереження токену сесії у сесії ASP.NET
               
                ViewBag.SessionToken = SessionManager.sessionToken;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Невірне ім'я користувача або пароль.");
                return View(model);
            }
        }
    }
}