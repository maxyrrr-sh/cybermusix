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
                cm.User.user = new User(model.Username, model.Password);
                cm.User.user.Register();
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
            cm.User.user = null;
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
            cm.User.user = new User(model.Username, model.Password);
            if (cm.User.user.Login())
            {
                // Генерація токену сесії
                SessionManager.sessionToken = SessionManager.GenerateToken(cm.User.user.getUsername());
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