using Microsoft.EntityFrameworkCore;
using mcbaMVC.Data;
using mcbaMVC.Models;

namespace mcbaMVC.Services
{
    public sealed class BillPayProcessor : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<BillPayProcessor> _logger;

        public BillPayProcessor(IServiceProvider sp, ILogger<BillPayProcessor> logger)
        {
            _sp = sp;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // small initial delay so the app can finish starting
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<MCBAContext>();
                    // ✅ call the public method so tests can exercise the exact same logic
                    await ProcessDueAsync(db, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "BillPay loop failed");
                }

                // run every minute
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }

        /// <summary>
        /// Processes all due scheduled bills. Public so unit tests can call it directly.
        /// </summary>
        public static async Task ProcessDueAsync(MCBAContext db, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            // 1) Get IDs of bills that are due and still allowed to be processed
            var dueIds = await db.BillPays
                .AsNoTracking() // initial read can be no-tracking
                .Where(b =>
                    b.ScheduleTimeUtc <= now &&
                    (b.Status == "S" || b.Status == "P") &&   // Scheduled or picked up
                    b.Status != "C")                           // not Cancelled
                .OrderBy(b => b.ScheduleTimeUtc)
                .Select(b => b.BillPayID)
                .Take(25)
                .ToListAsync(ct);

            if (dueIds.Count == 0)
                return;

            // 2) Re-fetch tracked and mark as Processing
            var toProcess = await db.BillPays
                .Where(b => dueIds.Contains(b.BillPayID))
                .ToListAsync(ct);

            foreach (var b in toProcess)
            {
                // Guard (in case status changed between reads)
                if (b.Status == "C")
                    continue;

                b.Status = "P";           // Processing
                b.LastAttemptUtc = now;
                b.LastError = null;
            }

            await db.SaveChangesAsync(ct);

            // 3) Process each bill one by one
            foreach (var id in dueIds)
                await ExecuteBillAsync(db, id, ct);
        }

        private static async Task ExecuteBillAsync(MCBAContext db, int billPayId, CancellationToken ct)
        {
            // Load the latest tracked record
            var bill = await db.BillPays.FirstOrDefaultAsync(b => b.BillPayID == billPayId, ct);
            if (bill is null)
                return;

            // If user cancelled after we queued it, skip
            if (bill.Status == "C")
                return;

            try
            {
                var acct = await db.Accounts.FirstAsync(a => a.AccountNumber == bill.AccountNumber, ct);

                // available balance logic (checking gets $500 overdraft headroom)
                var isChecking = string.Equals(acct.AccountType, "C", StringComparison.OrdinalIgnoreCase);
                var available  = isChecking ? Math.Max(0m, acct.Balance + 500m) : acct.Balance;

                if (available < bill.Amount)
                {
                    bill.Status = "F";                  // Failed
                    bill.LastError = "Insufficient funds.";
                    await db.SaveChangesAsync(ct);
                    return;
                }

                // Create BillPay transaction (no service fees)
                db.Transactions.Add(new Transaction
                {
                    TransactionType   = "B",            // BillPay
                    AccountNumber     = acct.AccountNumber,
                    Amount            = bill.Amount,
                    Comment           = "Scheduled bill payment",
                    TransactionTimeUtc= DateTime.UtcNow
                });

                // Debit account
                acct.Balance -= bill.Amount;

                if (bill.Period == "M")                 // monthly recurring
                {
                    bill.Status = "S";                   // back to Scheduled for next run
                    bill.ScheduleTimeUtc = bill.ScheduleTimeUtc.AddMonths(1);
                    bill.LastError = null;
                }
                else
                {
                    bill.Status = "D";                   // Done (Paid)
                    bill.LastError = null;
                }

                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                bill.Status = "F";
                bill.LastError = ex.Message;
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
