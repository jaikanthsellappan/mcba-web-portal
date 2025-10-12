using System;
using System.Linq;
using System.Threading.Tasks;
using mcbaMVC.Data;
using mcbaMVC.Models;
using mcbaMVC.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace mcbaMVC.Test;

public class BillPayProcessorTests
{
    [Fact]
    public async Task OneOff_Succeeds_SetsPaid_AndDeductsBalance()
    {
        using var db = TestHelpers.NewContext();
        var (_, acct) = TestHelpers.SeedCustomer(db);
        var payee = TestHelpers.SeedPayee(db);

        var bill = TestHelpers.NewBill(acct.AccountNumber, payee.PayeeID, 50m, DateTime.UtcNow.AddMinutes(-1));
        db.BillPays.Add(bill);
        await db.SaveChangesAsync();

        await BillPayProcessor.ProcessDueAsync(db, default);

        var updated = db.BillPays.First();
        Assert.Equal("D", updated.Status);
        Assert.Equal(50m, db.Transactions.First().Amount);
        Assert.Equal(50m, acct.Balance);
    }

    [Fact]
    public async Task Monthly_Success_ReschedulesNextMonth()
    {
        using var db = TestHelpers.NewContext();
        var (_, acct) = TestHelpers.SeedCustomer(db);
        var payee = TestHelpers.SeedPayee(db);

        var bill = TestHelpers.NewBill(acct.AccountNumber, payee.PayeeID, 10m, DateTime.UtcNow.AddMinutes(-1), "M");
        db.BillPays.Add(bill);
        await db.SaveChangesAsync();

        await BillPayProcessor.ProcessDueAsync(db, default);

        var updated = db.BillPays.First();
        Assert.Equal("S", updated.Status);
        Assert.True(updated.ScheduleTimeUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task InsufficientFunds_Fails_LeavesBalanceUnchanged()
    {
        using var db = TestHelpers.NewContext();
        var (_, acct) = TestHelpers.SeedCustomer(db);
        var payee = TestHelpers.SeedPayee(db);

        var bill = TestHelpers.NewBill(acct.AccountNumber, payee.PayeeID, 5000m, DateTime.UtcNow.AddMinutes(-1));
        db.BillPays.Add(bill);
        await db.SaveChangesAsync();

        await BillPayProcessor.ProcessDueAsync(db, default);

        var updated = db.BillPays.First();
        Assert.Equal("F", updated.Status);
        Assert.Contains("funds", updated.LastError ?? "", StringComparison.OrdinalIgnoreCase);
        Assert.Equal(100m, acct.Balance);
    }
}
