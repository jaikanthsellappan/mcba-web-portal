using Microsoft.AspNetCore.Mvc;
using mcbaMVC.ViewModels;
using mcbaMVC.Data;
using SimpleHashing.Net;

namespace mcbaMVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly MCBAContext _context;
        private readonly ISimpleHash _hasher;

        public LoginController(MCBAContext context)
        {
            _context = context;
            _hasher = new SimpleHash(); // uses PBKDF2 with Rfc2898DeriveBytes under the hood
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // ✅ Find login from DB
            var login = _context.Logins.FirstOrDefault(l => l.LoginID == model.LoginId);

            if (login == null)
            {
                TempData["LoginError"] = "Invalid login credentials";
                return View(model);
            }

            // ✅ Verify password with PBKDF2 hash
            bool isValid = _hasher.Verify(model.Password, login.PasswordHash);

            if (!isValid)
            {
                TempData["LoginError"] = "Invalid login credentials";
                return View(model);
            }

            // ✅ If valid, store session data
            HttpContext.Session.SetString("LoginID", login.LoginID);
            HttpContext.Session.SetInt32("CustomerID", login.CustomerID);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
