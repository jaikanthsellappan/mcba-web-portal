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
            _sp = sp; _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<MCBAContext>();
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

        private static async Task ProcessDueAsync(MCBAContext db, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            // 1) Get IDs of bills that ARE due and still allowed to be processed
            var dueIds = await db.BillPays
                .AsNoTracking() // this read is fine as NO-TRACKING (we re-fetch tracked below)
                .Where(b =>
                    b.ScheduleTimeUtc <= now &&
                    (b.Status == "S" || b.Status == "P") &&   // only scheduled or "already picked"
                    b.Status != "C" &&                        // never cancelled
                    b.Status != "B")                          // never blocked
                .OrderBy(b => b.ScheduleTimeUtc)
                .Select(b => b.BillPayID)
                .Take(25)
                .ToListAsync(ct);

            if (dueIds.Count == 0)
                return;

            // 2) Re-fetch the records TRACKED and mark them as Processing
            var toProcess = await db.BillPays
                .Where(b => dueIds.Contains(b.BillPayID))
                .ToListAsync(ct);

            foreach (var b in toProcess)
            {
                // Guard again (in case status changed between step 1 and step 2)
                if (b.Status == "C" || b.Status == "B")
                    continue;

                b.Status = "P"; // mark as processing
                b.LastAttemptUtc = now;
                b.LastError = null;
            }

            await db.SaveChangesAsync(ct);

            // 3) Process bills one by one. We pass IDs and re-load tracked inside ExecuteBillAsync.
            foreach (var id in dueIds)
                await ExecuteBillAsync(db, id, ct);
        }

        private static async Task ExecuteBillAsync(MCBAContext db, int billPayId, CancellationToken ct)
        {
            // Always re-load the bill TRACKED, ensuring we get the very latest status
            var bill = await db.BillPays.FirstOrDefaultAsync(b => b.BillPayID == billPayId, ct);
            if (bill is null)
                return;

            // *Hard guard* – if user cancelled or admin blocked after we queued it, skip
            if (bill.Status == "C" || bill.Status == "B")
                return;

            try
            {
                var acct = await db.Accounts.FirstAsync(a => a.AccountNumber == bill.AccountNumber, ct);

                var isChecking = string.Equals(acct.AccountType, "C", StringComparison.OrdinalIgnoreCase);
                var available  = isChecking ? Math.Max(0m, acct.Balance + 500m) : acct.Balance;

                if (available < bill.Amount)
                {
                    bill.Status = "F";          // failed
                    bill.LastError = "Insufficient funds.";
                    await db.SaveChangesAsync(ct);
                    return;
                }

                db.Transactions.Add(new Transaction
                {
                    TransactionType = "B",      // BillPay transaction
                    AccountNumber = acct.AccountNumber,
                    Amount = bill.Amount,
                    Comment = "Scheduled bill payment",
                    TransactionTimeUtc = DateTime.UtcNow
                });

                acct.Balance -= bill.Amount;

                if (bill.Period == "M")        // monthly recurring
                {
                    bill.Status = "S";          // reschedule for next month
                    bill.ScheduleTimeUtc = bill.ScheduleTimeUtc.AddMonths(1);
                }
                else
                {
                    bill.Status = "D";          // done/paid
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
