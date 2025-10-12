using mcbaMVC.Data;
using mcbaMVC.Models;
using mcbaMVC.ViewModels;                 // <-- IMPORTANT
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace mcbaMVC.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly MCBAContext _context;

        public TransactionsController(MCBAContext context) => _context = context;

        // ------------------ Constants (fees) ------------------
        private const decimal WithdrawFee = 0.01m;   // Rule 10
        private const decimal TransferFee = 0.05m;   // Rule 10

        // ------------------ Helpers ------------------

        private int? CurrentCustomerId() => HttpContext.Session.GetInt32("CustomerID");

        private async Task<List<Account>> GetCustomerAccountsAsync(int customerId) =>
            await _context.Accounts
                          .Where(a => a.CustomerID == customerId)
                          .OrderBy(a => a.AccountNumber)
                          .ToListAsync();

        // AccountType is a string in your model; derive a label from the first letter.
        private static string TypeLabel(string? t)
        {
            if (string.IsNullOrWhiteSpace(t)) return "—";
            var c = char.ToUpperInvariant(t[0]);
            return c == 'C' ? "Checking"
                 : c == 'S' ? "Savings"
                 : t;
        }

        private static bool IsChecking(Account a)
            => !string.IsNullOrWhiteSpace(a.AccountType)
               && char.ToUpperInvariant(a.AccountType[0]) == 'C';

        // Available balance according to business rules:
        // Savings: available = balance (min = 0)
        // Checking: available = balance + 500 (min = -500)
        private static decimal Available(Account a)
            => IsChecking(a) ? a.Balance + 500m : a.Balance;

        private SelectList BuildAccountSelectList(IEnumerable<Account> accounts, int? selected = null)
            => new SelectList(
                accounts.Select(a => new
                {
                    Value = a.AccountNumber,
                    Text  = $"{a.AccountNumber} ({TypeLabel(a.AccountType)}) - {a.Balance:C}"
                }),
                "Value", "Text", selected);

        // Count previous CHARGEABLE transactions for an account.
        // Chargeable = Withdraw OR OUTGOING Transfer.
        // We identify incoming credits by the comment "Transfer from #...".
        private async Task<int> CountChargeablesAsync(int accountNumber)
        {
            return await _context.Transactions
                .Where(t => t.AccountNumber == accountNumber &&
                            (t.TransactionType == "W" ||
                             (t.TransactionType == "T" &&
                              !(t.Comment != null && t.Comment.StartsWith("Transfer from")))))
                .CountAsync();
        }

        // Returns whether this account still has a free chargeable slot.
        private async Task<bool> HasFreeChargeableAsync(int accountNumber)
        {
            var count = await CountChargeablesAsync(accountNumber);
            return count < 2; // Rule 11: 2 free chargeable transactions per account
        }

        // =============== Deposit ===============

        public async Task<IActionResult> Deposit(int? accountNumber)
{
    var cid = CurrentCustomerId();
    if (cid is null) return RedirectToAction("Index", "Login");

    var accounts = await GetCustomerAccountsAsync(cid.Value);
    ViewBag.Accounts = BuildAccountSelectList(accounts, accountNumber);
    ViewBag.SelectedAccount = accountNumber;   // store for the view
    return View(new DepositVM());
}

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(DepositVM vm)
        {
            var cid = CurrentCustomerId();
            if (cid is null) return RedirectToAction("Index", "Login");

            var accounts = await GetCustomerAccountsAsync(cid.Value);
            var account  = accounts.FirstOrDefault(a => a.AccountNumber == vm.AccountNumber);
            if (account == null)
                ModelState.AddModelError(nameof(vm.AccountNumber), "Invalid account.");

            if (!ModelState.IsValid)
            {
                ViewBag.Accounts = BuildAccountSelectList(accounts, vm.AccountNumber);
                return View(vm);
            }

            var confirm = new ConfirmVM
            {
                Type              = TxnType.Deposit,
                AccountNumber     = account!.AccountNumber,
                AccountTypeLabel  = TypeLabel(account.AccountType),
                Amount            = vm.Amount,
                Fee               = 0m,                     // deposit has no fee (Rule 12)
                Comment           = vm.Comment,
                CurrentBalance    = account.Balance,
                AvailableBalance  = Available(account)
            };
            return View("Confirm", confirm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CommitDeposit(ConfirmVM c)
        {
            var cid = CurrentCustomerId();
            if (cid is null) return RedirectToAction("Index", "Login");

            if (c.Type != TxnType.Deposit || c.AccountNumber is null)
                return RedirectToAction(nameof(Deposit));

            var account = await _context.Accounts.FindAsync(c.AccountNumber.Value);
            if (account is null || account.CustomerID != cid)
                return RedirectToAction(nameof(Deposit));

            using var dbtx = await _context.Database.BeginTransactionAsync();
            try
            {
                account.Balance += c.Amount;

                _context.Transactions.Add(new Transaction
                {
                    AccountNumber       = account.AccountNumber,
                    TransactionType     = "D",
                    Amount              = c.Amount,
                    Comment             = c.Comment,
                    TransactionTimeUtc  = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await dbtx.CommitAsync();

                TempData["TxnOK"] = $"Deposited {c.Amount:C} to #{account.AccountNumber}.";
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                await dbtx.RollbackAsync();
                ModelState.AddModelError("", "Failed to complete deposit.");
                return RedirectToAction(nameof(Deposit));
            }
        }

        // =============== Withdraw ===============

        [HttpGet]
public async Task<IActionResult> Withdraw(int? accountNumber)
{
    var cid = CurrentCustomerId();
    if (cid is null) return RedirectToAction("Index", "Login");

    var accounts = await GetCustomerAccountsAsync(cid.Value);
    ViewBag.Accounts = BuildAccountSelectList(accounts, accountNumber);
    ViewBag.SelectedAccount = accountNumber;
    return View(new WithdrawVM());
}

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(WithdrawVM vm)
        {
            var cid = CurrentCustomerId();
            if (cid is null) return RedirectToAction("Index", "Login");

            var accounts = await GetCustomerAccountsAsync(cid.Value);
            var account  = accounts.FirstOrDefault(a => a.AccountNumber == vm.AccountNumber);
            if (account == null)
                ModelState.AddModelError(nameof(vm.AccountNumber), "Invalid account.");

            if (account != null)
            {
                var isFree = await HasFreeChargeableAsync(account.AccountNumber);
                var fee    = isFree ? 0m : WithdrawFee;

                // Ensure amount + fee <= Available(account)
                if (vm.Amount + fee > Available(account))
                    ModelState.AddModelError(nameof(vm.Amount), $"Insufficient available funds (incl. {fee:C} fee).");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Accounts = BuildAccountSelectList(accounts, vm.AccountNumber);
                return View(vm);
            }

            var isFreeConfirm = await HasFreeChargeableAsync(account!.AccountNumber);
            var feeConfirm    = isFreeConfirm ? 0m : WithdrawFee;

            var confirm = new ConfirmVM
            {
                Type               = TxnType.Withdraw,
                AccountNumber      = account.AccountNumber,
                AccountTypeLabel   = TypeLabel(account.AccountType),
                Amount             = vm.Amount,
                Fee                = feeConfirm,            // shown on confirm
                Comment            = vm.Comment,
                CurrentBalance     = account.Balance,
                AvailableBalance   = Available(account)
            };
            return View("Confirm", confirm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CommitWithdraw(ConfirmVM c)
        {
            var cid = CurrentCustomerId();
            if (cid is null) return RedirectToAction("Index", "Login");

            if (c.Type != TxnType.Withdraw || c.AccountNumber is null)
                return RedirectToAction(nameof(Withdraw));

            var account = await _context.Accounts.FindAsync(c.AccountNumber.Value);
            if (account is null || account.CustomerID != cid)
                return RedirectToAction(nameof(Withdraw));

            var isFree = await HasFreeChargeableAsync(account.AccountNumber);
            var fee    = isFree ? 0m : WithdrawFee;

            if (c.Amount + fee > Available(account))
                return RedirectToAction(nameof(Withdraw));

            using var dbtx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Debit
                account.Balance -= c.Amount;
                _context.Transactions.Add(new Transaction
                {
                    AccountNumber       = account.AccountNumber,
                    TransactionType     = "W",
                    Amount              = c.Amount,
                    Comment             = c.Comment,
                    TransactionTimeUtc  = DateTime.UtcNow
                });

                // Fee (if any)
                if (fee > 0m)
                {
                    account.Balance -= fee;
                    _context.Transactions.Add(new Transaction
                    {
                        AccountNumber       = account.AccountNumber,
                        TransactionType     = "S",
                        Amount              = fee,
                        Comment             = "Withdraw service fee",
                        TransactionTimeUtc  = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                await dbtx.CommitAsync();

                TempData["TxnOK"] = $"Withdrew {c.Amount:C}" + (fee > 0 ? $" (+ {fee:C} fee)" : "")
                                  + $" from #{account.AccountNumber}.";
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                await dbtx.RollbackAsync();
                ModelState.AddModelError("", "Failed to complete withdrawal.");
                return RedirectToAction(nameof(Withdraw));
            }
        }

        // =============== Transfer ===============
[HttpGet]
public async Task<IActionResult> Transfer(int? fromAccountNumber)
{
    var cid = CurrentCustomerId();
    if (cid is null) return RedirectToAction("Index", "Login");

    var accounts = await GetCustomerAccountsAsync(cid.Value);
    ViewBag.FromAccounts = BuildAccountSelectList(accounts, fromAccountNumber);
    ViewBag.SelectedFrom = fromAccountNumber;
    return View(new TransferVM());
}

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(TransferVM vm)
        {
            var cid = CurrentCustomerId();
            if (cid is null) return RedirectToAction("Index", "Login");

            var fromAcc = await _context.Accounts.FindAsync(vm.FromAccountNumber);
            var toAcc   = await _context.Accounts.FindAsync(vm.ToAccountNumber);

            if (fromAcc == null || fromAcc.CustomerID != cid)
                ModelState.AddModelError(nameof(vm.FromAccountNumber), "Invalid source account.");

            if (toAcc == null)
                ModelState.AddModelError(nameof(vm.ToAccountNumber), "Destination account not found.");

            if (fromAcc != null)
            {
                var isFree = await HasFreeChargeableAsync(fromAcc.AccountNumber);
                var fee    = isFree ? 0m : TransferFee;

                if (vm.Amount + fee > Available(fromAcc))
                    ModelState.AddModelError(nameof(vm.Amount), $"Insufficient available funds (incl. {fee:C} fee).");
            }

            if (fromAcc != null && toAcc != null && fromAcc.AccountNumber == toAcc.AccountNumber)
                ModelState.AddModelError(nameof(vm.ToAccountNumber), "Source and destination cannot be the same.");

            if (!ModelState.IsValid)
            {
                var accounts = await GetCustomerAccountsAsync(cid.Value);
                ViewBag.FromAccounts = BuildAccountSelectList(accounts, vm.FromAccountNumber);
                return View(vm);
            }

            var isFreeConfirm = await HasFreeChargeableAsync(fromAcc!.AccountNumber);
            var feeConfirm    = isFreeConfirm ? 0m : TransferFee;

            var confirm = new ConfirmVM
            {
                Type                  = TxnType.Transfer,
                FromAccountNumber     = fromAcc.AccountNumber,
                FromTypeLabel         = TypeLabel(fromAcc.AccountType),
                FromCurrentBalance    = fromAcc.Balance,
                FromAvailableBalance  = Available(fromAcc),

                ToAccountNumber       = toAcc!.AccountNumber,
                ToTypeLabel           = TypeLabel(toAcc.AccountType),
                ToCurrentBalance      = toAcc.Balance,

                Amount = vm.Amount,
                Fee    = feeConfirm,               // shown on confirm
                Comment = vm.Comment
            };

            return View("Confirm", confirm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CommitTransfer(ConfirmVM c)
        {
            var cid = CurrentCustomerId();
            if (cid is null) return RedirectToAction("Index", "Login");

            if (c.Type != TxnType.Transfer ||
                c.FromAccountNumber is null || c.ToAccountNumber is null)
                return RedirectToAction(nameof(Transfer));

            var from = await _context.Accounts.FindAsync(c.FromAccountNumber.Value);
            var to   = await _context.Accounts.FindAsync(c.ToAccountNumber.Value);

            if (from is null || to is null || from.CustomerID != cid)
                return RedirectToAction(nameof(Transfer));

            var isFree = await HasFreeChargeableAsync(from.AccountNumber);
            var fee    = isFree ? 0m : TransferFee;

            if (c.Amount + fee > Available(from) || from.AccountNumber == to.AccountNumber)
                return RedirectToAction(nameof(Transfer));

            using var dbtx = await _context.Database.BeginTransactionAsync();
            try
            {
                from.Balance -= c.Amount;
                to.Balance   += c.Amount;

                // from: outgoing transfer (debit)
                _context.Transactions.Add(new Transaction
                {
                    AccountNumber              = from.AccountNumber,
                    TransactionType            = "T",
                    Amount                     = c.Amount,
                    Comment                    = c.Comment,
                    DestinationAccountNumber   = to.AccountNumber,
                    TransactionTimeUtc         = DateTime.UtcNow
                });

                // to: incoming transfer (credit)
                _context.Transactions.Add(new Transaction
                {
                    AccountNumber              = to.AccountNumber,
                    TransactionType            = "T",
                    Amount                     = c.Amount,
                    Comment                    = c.Comment,
                    DestinationAccountNumber   = from.AccountNumber,
                    TransactionTimeUtc         = DateTime.UtcNow
                });

                // fee on 'from' if not free
                if (fee > 0m)
                {
                    from.Balance -= fee;
                    _context.Transactions.Add(new Transaction
                    {
                        AccountNumber       = from.AccountNumber,
                        TransactionType     = "S",
                        Amount              = fee,
                        Comment             = "Transfer service fee",
                        TransactionTimeUtc  = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                await dbtx.CommitAsync();

                TempData["TxnOK"] = $"Transferred {c.Amount:C}" + (fee > 0 ? $" (+ {fee:C} fee)" : "")
                                  + $" from #{from.AccountNumber} to #{to.AccountNumber}.";
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                await dbtx.RollbackAsync();
                ModelState.AddModelError("", "Failed to complete transfer.");
                return RedirectToAction(nameof(Transfer));
            }
        }

        // ---- Cancel from the confirm page ----
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Cancel() => RedirectToAction("Index", "Home");
    }
}
