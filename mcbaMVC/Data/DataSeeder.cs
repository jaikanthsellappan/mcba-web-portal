using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using mcbaMVC.DTO;
using mcbaMVC.Models;

namespace mcbaMVC.Data
{
    public class DataSeeder
    {
        private readonly MCBAContext _context;
        private readonly HttpClient _httpClient;

        public DataSeeder(MCBAContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Seeds Customers (+ Logins, Accounts, Transactions) from the course JSON feed.
        /// Safe to call multiple times (no-op if customers already exist).
        /// </summary>
        public async Task SeedCustomersAsync()
        {
            if (await _context.Customers.AsNoTracking().AnyAsync()) return;

            var url = "https://coreteaching01.csit.rmit.edu.au/~e103884/wdt/services/customers/";
            var json = await _httpClient.GetStringAsync(url);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var customers = JsonSerializer.Deserialize<List<CustomerDTO>>(json, options);
            if (customers is null || customers.Count == 0) return;

            foreach (var c in customers)
            {
                var customer = new Customer
                {
                    CustomerID        = c.CustomerID,
                    Name              = c.Name,
                    Address           = c.Address,
                    City              = c.City,
                    State             = c.State,
                    Postcode          = c.Postcode,
                    Mobile            = c.Mobile,
                    CustomerAccounts  = new List<Account>()
                };

                // Attach login
                customer.Login = new Login
                {
                    LoginID      = c.Login.LoginID,
                    CustomerID   = c.CustomerID,
                    PasswordHash = c.Login.PasswordHash
                };

                // Accounts + initial transactions (as deposits to build the starting balance)
                foreach (var acc in c.Accounts)
                {
                    var account = new Account
                    {
                        AccountNumber        = acc.AccountNumber,
                        AccountType          = acc.AccountType, // "S" or "C"
                        CustomerID           = c.CustomerID,
                        RelatedTransactions  = new List<Transaction>()
                    };

                    decimal balance = 0m;

                    foreach (var t in acc.Transactions)
                    {
                        // Example format from feed: 1/1/2019 12:00:00 AM
                        var txTime = DateTime.ParseExact(
                            t.TransactionTimeUtc,
                            "d/M/yyyy h:mm:ss tt",
                            System.Globalization.CultureInfo.InvariantCulture);

                        account.RelatedTransactions.Add(new Transaction
                        {
                            TransactionType     = "D",         // seed as deposit
                            AccountNumber       = acc.AccountNumber,
                            Amount              = t.Amount,
                            TransactionTimeUtc  = txTime,
                            Comment             = null
                        });

                        balance += t.Amount;
                    }

                    account.Balance = balance;
                    customer.CustomerAccounts.Add(account);
                }

                _context.Customers.Add(customer);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds a few dummy Payees so the BillPay "Payee" dropdown is populated.
        /// Safe to call multiple times (no-op if any payees already exist).
        /// </summary>
        public async Task SeedPayeesAsync()
        {
            if (await _context.Payees.AsNoTracking().AnyAsync()) return;

            var payees = new[]
            {
                new Payee
                {
                    Name     = "Rob Pty Ltd",
                    Address  = "1 Rob Street",
                    City     = "Melbourne",
                    State    = "VIC",
                    Postcode = "3000",
                    // Spec: Payee phone uses landline format 0X XXXX XXXX
                    Phone    = "03 9123 4567"
                },
                new Payee
                {
                    Name     = "Bob Services",
                    Address  = "22 Bob Avenue",
                    City     = "Sydney",
                    State    = "NSW",
                    Postcode = "2000",
                    Phone    = "02 7000 1111"
                },
                new Payee
                {
                    Name     = "Hob Electric",
                    Address  = "7 Hob Road",
                    City     = "Brisbane",
                    State    = "QLD",
                    Postcode = "4000",
                    Phone    = "07 5555 9999"
                }
            };

            _context.Payees.AddRange(payees);
            await _context.SaveChangesAsync();
        }
    }
}
