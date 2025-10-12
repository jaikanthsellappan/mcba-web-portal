using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mcbaMVC.Controllers;
using mcbaMVC.Data;
using mcbaMVC.Infrastructure;
using mcbaMVC.Models;
using Xunit;

namespace mcbaMVC.Test;

public class BillPayControllerTests
{
    private static DefaultHttpContext MockHttpContext(int customerId)
    {
        var context = new DefaultHttpContext();
        var session = new TestSession();
        session.SetInt32(SessionKeys.LoggedInCustomerId, customerId);
        context.Session = session;
        return context;
    }

    [Fact]
    public async Task Create_PastDate_ReturnsViewWithError()
    {
        using var db = TestHelpers.NewContext();
        var (cust, acct) = TestHelpers.SeedCustomer(db);
        var payee = TestHelpers.SeedPayee(db);

        var controller = new BillPayController(db)
        {
            ControllerContext = new ControllerContext { HttpContext = MockHttpContext(cust.CustomerID) }
        };

        var model = new BillPay
        {
            AccountNumber = acct.AccountNumber,
            PayeeID = payee.PayeeID,
            Amount = 5,
            Period = "O"
        };

        var result = await controller.Create(model, DateTime.Now.AddMinutes(-5).ToString("yyyy-MM-ddTHH:mm"));
        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Create_Valid_RedirectsToIndex()
    {
        using var db = TestHelpers.NewContext();
        var (cust, acct) = TestHelpers.SeedCustomer(db);
        var payee = TestHelpers.SeedPayee(db);

        var controller = new BillPayController(db)
        {
            ControllerContext = new ControllerContext { HttpContext = MockHttpContext(cust.CustomerID) }
        };

        var model = new BillPay
        {
            AccountNumber = acct.AccountNumber,
            PayeeID = payee.PayeeID,
            Amount = 25,
            Period = "O"
        };

        var result = await controller.Create(model, DateTime.Now.AddMinutes(10).ToString("yyyy-MM-ddTHH:mm"));
        Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(db.BillPays);
    }
}
