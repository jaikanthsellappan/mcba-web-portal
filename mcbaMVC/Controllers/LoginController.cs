using Microsoft.AspNetCore.Mvc;
using mcbaMVC.ViewModels;
using mcbaMVC.Data;
using mcbaMVC.Infrastructure;
using SimpleHashing.Net;

namespace mcbaMVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly MCBAContext _context;
        private readonly ISimpleHash _hasher = new SimpleHash(); // PBKDF2

        public LoginController(MCBAContext context) => _context = context;

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var login = _context.Logins.FirstOrDefault(l => l.LoginID == model.LoginId);
            if (login is null || !_hasher.Verify(model.Password, login.PasswordHash))
            {
                TempData["LoginError"] = "Invalid login credentials";
                return View(model);
            }

            var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == login.CustomerID);
            var name = customer?.Name ?? login.LoginID;

            // ✅ Unified session keys
            HttpContext.Session.SetInt32(SessionKeys.LoggedInCustomerId, login.CustomerID);
            HttpContext.Session.SetString(SessionKeys.LoggedInLoginId, login.LoginID);
            HttpContext.Session.SetString(SessionKeys.LoggedInName, name);

            // 🔁 Back-compat with any code that still checks "CustomerID"
            HttpContext.Session.SetInt32("CustomerID", login.CustomerID);

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
