using mcbaMVC.Data;
using mcbaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace mcbaMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MCBAContext _context;

        public HomeController(ILogger<HomeController> logger, MCBAContext context)
        {
            _logger = logger;
            _context = context;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (customerId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var customer = _context.Customers
                .Include(c => c.CustomerAccounts)
                .FirstOrDefault(c => c.CustomerID == customerId);

            if (customer == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Login");
            }

            ViewBag.CustomerName = customer.Name;
            ViewBag.Accounts = customer.CustomerAccounts;

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

        // ✅ New Logout action
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}
