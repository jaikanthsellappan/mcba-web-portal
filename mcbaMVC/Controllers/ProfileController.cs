using mcbaMVC.Data;
using mcbaMVC.Models;
using mcbaMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleHashing.Net;
using System.Threading.Tasks;

namespace mcbaMVC.Controllers
{
    public class ProfileController : Controller
    {
        private readonly MCBAContext _context;
        private readonly ISimpleHash _hasher;

        public ProfileController(MCBAContext context)
        {
            _context = context;
            _hasher = new SimpleHash();
        }

        private int? CurrentCustomerId() => HttpContext.Session.GetInt32("CustomerID");

        // ---------------- View Profile ----------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cid = CurrentCustomerId();
            if (cid is null) return RedirectToAction("Index", "Login");

            var customer = await _context.Customers
                                         .Include(c => c.Login)
                                         .FirstOrDefaultAsync(c => c.CustomerID == cid);

            if (customer == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Login");
            }

            return View(customer);
        }

        // ---------------- Edit Profile ----------------
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var cid = CurrentCustomerId();
            if (cid is null) return RedirectToAction("Index", "Login");

            var customer = await _context.Customers.FindAsync(cid);
            if (customer == null) return RedirectToAction("Index");

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var cid = CurrentCustomerId();
            var customer = await _context.Customers.FindAsync(cid);
            if (customer == null)
                return RedirectToAction("Index", "Login");

            _context.Entry(customer).CurrentValues.SetValues(model);
            await _context.SaveChangesAsync();

            TempData["ProfileUpdated"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }





        // ---------------- Change Password ----------------
        [HttpGet]
        public IActionResult ChangePassword() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var cid = CurrentCustomerId();
            if (cid is null) return RedirectToAction("Index", "Login");

            if (string.IsNullOrWhiteSpace(currentPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "New passwords do not match.";
                return View();
            }

            var login = await _context.Logins.FirstOrDefaultAsync(l => l.CustomerID == cid);
            if (login == null)
            {
                ViewBag.Error = "Login not found.";
                return View();
            }

            if (!_hasher.Verify(currentPassword, login.PasswordHash))
            {
                ViewBag.Error = "Current password is incorrect.";
                return View();
            }

            // Hash and update
            login.PasswordHash = _hasher.Compute(newPassword);
            await _context.SaveChangesAsync();

            TempData["PasswordChanged"] = "Password updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
