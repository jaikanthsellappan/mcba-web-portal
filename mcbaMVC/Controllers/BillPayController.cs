using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mcbaMVC.Data;
using mcbaMVC.Models;
using mcbaMVC.Infrastructure;

namespace mcbaMVC.Controllers
{
    public sealed class BillPayController : Controller
    {
        private readonly MCBAContext _db;
        public BillPayController(MCBAContext db) => _db = db;

        // GET: /BillPay
        public async Task<IActionResult> Index()
        {
            var cid = HttpContext.Session.GetInt32(SessionKeys.LoggedInCustomerId);
            if (cid is null) return RedirectToAction("Index", "Login");

            var items = await _db.BillPays
                .Include(b => b.Payee)
                .Include(b => b.Account)
                .Where(b => b.Account.CustomerID == cid.Value)
                .OrderByDescending(b => b.ScheduleTimeUtc)
                .AsNoTracking()
                .ToListAsync();

            return View(items);
        }

        // GET: /BillPay/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? fromAccountNumber = null)
        {
            var cid = HttpContext.Session.GetInt32(SessionKeys.LoggedInCustomerId);
            if (cid is null) return RedirectToAction("Index", "Login");

            await LoadDropdownsAsync(cid.Value);
            return View(new BillPay
            {
                AccountNumber = fromAccountNumber ?? 0,
                Period = "O",
                Status = "S"
            });
        }

        // POST: /BillPay/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BillPay input, string scheduleLocal)
        {
            var cid = HttpContext.Session.GetInt32(SessionKeys.LoggedInCustomerId);
            if (cid is null) return RedirectToAction("Index", "Login");

            // 1) account must be one of the user's accounts
            var account = await _db.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == input.AccountNumber && a.CustomerID == cid.Value);
            if (account is null)
                ModelState.AddModelError(nameof(BillPay.AccountNumber), "Please choose one of your accounts.");

            // 2) payee must exist
            var payeeExists = await _db.Payees.AnyAsync(p => p.PayeeID == input.PayeeID);
            if (!payeeExists)
                ModelState.AddModelError(nameof(BillPay.PayeeID), "Please choose a valid payee.");
            else
                input.Payee = await _db.Payees.FindAsync(input.PayeeID);

            // 3) amount
            if (input.Amount <= 0)
                ModelState.AddModelError(nameof(BillPay.Amount), "Amount must be positive.");

            // 4) period
            if (input.Period != "O" && input.Period != "M")
                ModelState.AddModelError(nameof(BillPay.Period), "Period must be 'O' (Once) or 'M' (Monthly).");

            // 5) date/time (HTML datetime-local posts as yyyy-MM-ddTHH:mm)
            if (!DateTime.TryParseExact(scheduleLocal,
                                        "yyyy-MM-ddTHH:mm",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.AssumeLocal,
                                        out var local))
            {
                ModelState.AddModelError(nameof(BillPay.ScheduleTimeUtc), "Invalid date/time.");
            }
            else
            {
                if (local < DateTime.Now.AddMinutes(-1))
                    ModelState.AddModelError(nameof(BillPay.ScheduleTimeUtc), "Schedule time cannot be in the past.");

                input.ScheduleTimeUtc = local.ToUniversalTime();
            }

            // (Optional) If you want to block scheduling that can't be afforded *right now*:
            // var isChecking = account?.AccountType?.Equals("C", StringComparison.OrdinalIgnoreCase) == true;
            // var available = isChecking ? Math.Max(0m, account!.Balance + 500m) : account!.Balance;
            // if (input.Amount > available) ModelState.AddModelError(nameof(BillPay.Amount), "Insufficient available balance.");

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(cid.Value);
                return View(input);
            }

            input.Status = "S";
            _db.BillPays.Add(input);
            await _db.SaveChangesAsync();

            TempData["Toast"] = "Bill payment scheduled.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /BillPay/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var cid = HttpContext.Session.GetInt32(SessionKeys.LoggedInCustomerId);
            if (cid is null) return RedirectToAction("Index", "Login");

            var bill = await _db.BillPays
                .Include(b => b.Account)
                .FirstOrDefaultAsync(b => b.BillPayID == id && b.Account.CustomerID == cid.Value);

            if (bill is not null && (bill.Status == "S" || bill.Status == "B"))
            {
                bill.Status = "C";
                bill.LastAttemptUtc = DateTime.UtcNow;
                bill.LastError = "Cancelled by customer";
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync(int customerId)
        {
            ViewBag.Accounts = await _db.Accounts
                .Where(a => a.CustomerID == customerId)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();

            ViewBag.Payees = await _db.Payees
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
    }
}
