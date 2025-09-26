using mcbaMVC.DTO;
using mcbaMVC.Models;
using System.Text.Json;

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

        public async Task SeedCustomersAsync()
        {
            // Skip if customers already exist
            if (_context.Customers.Any()) return;

            var url = "https://coreteaching01.csit.rmit.edu.au/~e103884/wdt/services/customers/";
            var response = await _httpClient.GetStringAsync(url);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var customers = JsonSerializer.Deserialize<List<CustomerDTO>>(response, options);

            if (customers == null) return;

            foreach (var c in customers)
            {
                // Initialize customer with empty accounts list
                var customer = new Customer
                {
                    CustomerID = c.CustomerID,
                    Name = c.Name,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    Postcode = c.Postcode,
                    Mobile = c.Mobile,
                    CustomerAccounts = new List<Account>()
                };

                // Create login and attach it via navigation property
                var login = new Login
                {
                    LoginID = c.Login.LoginID,
                    CustomerID = c.CustomerID,
                    PasswordHash = c.Login.PasswordHash
                };

                customer.Login = login;

                // Process accounts and transactions
                foreach (var acc in c.Accounts)
                {
                    var account = new Account
                    {
                        AccountNumber = acc.AccountNumber,
                        AccountType = acc.AccountType,
                        CustomerID = c.CustomerID,
                        RelatedTransactions = new List<Transaction>()
                    };

                    decimal balance = 0;

                    foreach (var t in acc.Transactions)
                    {
                        // Parse string time → DateTime
                        DateTime txTime = DateTime.ParseExact(
                            t.TransactionTimeUtc,
                            "d/M/yyyy h:mm:ss tt",
                            System.Globalization.CultureInfo.InvariantCulture);

                        var transaction = new Transaction
                        {
                            TransactionType = "D", // Always Deposit
                            AccountNumber = acc.AccountNumber,
                            Amount = t.Amount,
                            TransactionTimeUtc = txTime
                        };

                        balance += t.Amount;
                        account.RelatedTransactions.Add(transaction);
                    }

                    account.Balance = balance;
                    customer.CustomerAccounts.Add(account);
                }

                // Add the whole customer graph (Customer → Login → Accounts → Transactions)
                _context.Customers.Add(customer);
            }

            await _context.SaveChangesAsync();
        }
    }
}