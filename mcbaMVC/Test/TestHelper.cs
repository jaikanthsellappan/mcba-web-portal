using System;
using Microsoft.EntityFrameworkCore;
using mcbaMVC.Data;
using mcbaMVC.Models;

namespace mcbaMVC.Test;

internal static class TestHelpers
{
    public static MCBAContext NewContext()
    {
        var options = new DbContextOptionsBuilder<MCBAContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new MCBAContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    public static (Customer customer, Account savings) SeedCustomer(MCBAContext db)
    {
        var cust = new Customer { CustomerID = 1, Name = "Nethra" };
        var acct = new Account { AccountNumber = 4100, CustomerID = 1, AccountType = "S", Balance = 100m };
        db.Customers.Add(cust);
        db.Accounts.Add(acct);
        db.SaveChanges();
        return (cust, acct);
    }

    public static Payee SeedPayee(MCBAContext db)
    {
        var payee = new Payee { PayeeID = 1, Name = "Test Payee", Address = "1 St", City = "Melbourne", State = "VIC", Postcode = "3000", Phone = "000" };
        db.Payees.Add(payee);
        db.SaveChanges();
        return payee;
    }

    public static BillPay NewBill(int acct, int payee, decimal amt, DateTime timeUtc, string period = "O")
        => new()
        {
            AccountNumber = acct,
            PayeeID = payee,
            Amount = amt,
            ScheduleTimeUtc = timeUtc,
            Period = period,
            Status = "S"
        };
}
