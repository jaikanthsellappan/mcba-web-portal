using Microsoft.AspNetCore.Mvc;
using mcbaMVC.ViewModels;

namespace mcbaMVC.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Return with validation errors
            }

            // Dummy check - replace later with DB/auth logic
            if (model.LoginId == "12345678" && model.Password == "password")
            {
                // Redirect to home/dashboard if success
                return RedirectToAction("Index", "Home");
            }

            TempData["LoginError"] = "Invalid login credentials";
            return View(model);
        }
    }
}
