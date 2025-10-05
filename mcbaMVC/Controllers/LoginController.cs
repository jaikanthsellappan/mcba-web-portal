using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mcbaMVC.ViewModels;
using mcbaMVC.Data;
using mcbaMVC.Infrastructure;   // SessionKeys
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
            _hasher  = new SimpleHash(); // PBKDF2
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var login = _context.Logins
                                .Include(l => l.Customer)
                                .FirstOrDefault(l => l.LoginID == model.LoginId);

            if (login is null || !_hasher.Verify(model.Password, login.PasswordHash))
            {
                TempData["LoginError"] = "Invalid login credentials";
                return View(model);
            }

            HttpContext.Session.SetInt32(SessionKeys.LoggedInCustomerId, login.CustomerID);
            HttpContext.Session.SetString(SessionKeys.LoggedInLoginId,   login.LoginID);
            HttpContext.Session.SetString(SessionKeys.LoggedInName,      login.Customer?.Name ?? "Customer");

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
