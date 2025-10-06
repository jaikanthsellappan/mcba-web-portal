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

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }

        private static async Task ProcessDueAsync(MCBAContext db, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            var due = await db.BillPays
                .Where(b => b.Status == "S" && b.ScheduleTimeUtc <= now)
                .OrderBy(b => b.ScheduleTimeUtc)
                .Take(25)
                .ToListAsync(ct);

            if (due.Count == 0) return;

            foreach (var b in due)
            {
                b.Status = "P"; // processing
                b.LastAttemptUtc = now;
                b.LastError = null;
            }
            await db.SaveChangesAsync(ct);

            foreach (var bill in due)
                await ExecuteBillAsync(db, bill, ct);
        }

        private static async Task ExecuteBillAsync(MCBAContext db, BillPay bill, CancellationToken ct)
        {
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

                if (bill.Period == "M")        // monthly
                {
                    bill.Status = "S";          // reschedule
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
