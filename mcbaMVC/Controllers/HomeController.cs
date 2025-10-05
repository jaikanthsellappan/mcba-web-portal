using mcbaMVC.Data;
using mcbaMVC.Models;
using mcbaMVC.Infrastructure;   // <-- SessionKeys
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
            var customerId = HttpContext.Session.GetInt32(SessionKeys.LoggedInCustomerId);
            if (customerId is null)
                return RedirectToAction("Index", "Login");

            var customer = _context.Customers
                .Include(c => c.CustomerAccounts)
                .AsNoTracking()
                .FirstOrDefault(c => c.CustomerID == customerId.Value);

            if (customer is null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Login");
            }

            ViewBag.CustomerName = customer.Name;
            ViewBag.Accounts = customer.CustomerAccounts; // your Index.cshtml handles IEnumerable<Account>

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}
