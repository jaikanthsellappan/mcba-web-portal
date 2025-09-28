using Microsoft.AspNetCore.Mvc;

namespace mcbaMVC.Controllers
{
    public class LoginController : Controller
    {
        // GET: /Login
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Login
        [HttpPost]
        public IActionResult Index(string loginId, string password)
        {
            // For now, just a placeholder
            // Your friend will implement authentication here.
            
            // Redirect to Dashboard (Accounts page) after successful login
            return RedirectToAction("Index", "Accounts");
        }

        // GET: /Login/Logout
        public IActionResult Logout()
        {
            // TODO: Add session clear later
            return RedirectToAction("Index", "Home");
        }
    }
}
