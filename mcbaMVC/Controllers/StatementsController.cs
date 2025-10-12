using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mcbaMVC.Data;
using mcbaMVC.Models;
using mcbaMVC.Infrastructure;   // SessionKeys

namespace mcbaMVC.Controllers
{
    public sealed class StatementsController : Controller
    {
        private readonly MCBAContext _db;
        private const int PageSize = 4;

        public StatementsController(MCBAContext db)
        {
            _db = db;
        }
        [HttpGet]
public async Task<IActionResult> SelectAccount()
{
    var cid = HttpContext.Session.GetInt32(SessionKeys.LoggedInCustomerId);
    if (cid is null) 
        return RedirectToAction("Index", "Login");

    var accounts = await _db.Accounts
        .AsNoTracking()
        .Where(a => a.CustomerID == cid)
        .OrderBy(a => a.AccountNumber)
        .ToListAsync();

    if (accounts.Count == 0)
        return RedirectToAction("Index", "Home");

    return View(accounts);
}

        // GET /Statements?accountNumber=4100&page=1
        public async Task<IActionResult> Index(int? accountNumber, int page = 1)
        {
            var cid = HttpContext.Session.GetInt32(SessionKeys.LoggedInCustomerId);
            if (cid is null) return RedirectToAction("Index", "Login");
            var customerId = cid.Value;

            var baseQ = _db.Accounts.AsNoTracking()
                                    .Where(a => a.CustomerID == customerId);

            var acct = accountNumber is null
                ? await baseQ.OrderBy(a => a.AccountNumber).FirstOrDefaultAsync()
                : await baseQ.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber.Value);

            if (acct is null)
                return RedirectToAction("Index", "Home");

            var txQ = _db.Transactions.AsNoTracking()
                                      .Where(t => t.AccountNumber == acct.AccountNumber)
                                      .OrderByDescending(t => t.TransactionTimeUtc);

            var total = await txQ.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            page = Math.Clamp(page, 1, totalPages);

            var pageTx = await txQ.Skip((page - 1) * PageSize)
                                  .Take(PageSize)
                                  .ToListAsync();

            var rows = pageTx.Select(t => new TransactionRowVM
            {
                TransactionID = t.TransactionID,
                TransactionType = t.TransactionType,
                AccountNumber = t.AccountNumber,
                DestinationAccountNumber = t.DestinationAccountNumber,
                Amount = t.Amount,
                TransactionTimeLocal = DateTime.SpecifyKind(t.TransactionTimeUtc, DateTimeKind.Utc).ToLocalTime(),
                Comment = string.IsNullOrWhiteSpace(t.Comment) ? null : t.Comment
            }).ToList();

            var isChecking = string.Equals(acct.AccountType, "C", StringComparison.OrdinalIgnoreCase);
            var current    = acct.Balance;
            var available  = isChecking ? Math.Max(0m, current + 500m) : current;

            var vm = new StatementsVM
            {
                AccountNumber     = acct.AccountNumber,
                AccountType       = isChecking ? "Checking"
                                   : (string.Equals(acct.AccountType, "S", StringComparison.OrdinalIgnoreCase) ? "Savings" : acct.AccountType),
                CurrentBalance    = current,
                AvailableBalance  = available,
                Page              = page,
                TotalPages        = totalPages,
                Transactions      = rows
            };

            return View(vm);
        }

        // Print-friendly page (no paging)
        public async Task<IActionResult> Print(int accountNumber)
        {
            var cid = HttpContext.Session.GetInt32(SessionKeys.LoggedInCustomerId);
            if (cid is null) return RedirectToAction("Index", "Login");
            var customerId = cid.Value;

            var acct = await _db.Accounts.AsNoTracking()
                                         .FirstOrDefaultAsync(a => a.CustomerID == customerId &&
                                                                   a.AccountNumber == accountNumber);
            if (acct is null)
                return RedirectToAction("Index");

            var items = await _db.Transactions.AsNoTracking()
                                              .Where(t => t.AccountNumber == accountNumber)
                                              .OrderByDescending(t => t.TransactionTimeUtc)
                                              .ToListAsync();

            var rows = items.Select(t => new TransactionRowVM
            {
                TransactionID = t.TransactionID,
                TransactionType = t.TransactionType,
                AccountNumber = t.AccountNumber,
                DestinationAccountNumber = t.DestinationAccountNumber,
                Amount = t.Amount,
                TransactionTimeLocal = DateTime.SpecifyKind(t.TransactionTimeUtc, DateTimeKind.Utc).ToLocalTime(),
                Comment = string.IsNullOrWhiteSpace(t.Comment) ? null : t.Comment
            }).ToList();

            var isChecking = string.Equals(acct.AccountType, "C", StringComparison.OrdinalIgnoreCase);
            var current    = acct.Balance;
            var available  = isChecking ? Math.Max(0m, current + 500m) : current;

            ViewData["DocTitle"] = $"TransactionHistory_Account_{accountNumber}";

            var vm = new StatementsVM
            {
                AccountNumber     = acct.AccountNumber,
                AccountType       = isChecking ? "Checking"
                                   : (string.Equals(acct.AccountType, "S", StringComparison.OrdinalIgnoreCase) ? "Savings" : acct.AccountType),
                CurrentBalance    = current,
                AvailableBalance  = available,
                Page              = 1,
                TotalPages        = 1,
                Transactions      = rows
            };

            return View("Print", vm);
        }
    }
}
