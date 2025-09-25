using mcbaMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace mcbaMVC.Data
{
    public class MCBAContext : DbContext
    {
        public MCBAContext(DbContextOptions<MCBAContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Login> Logins { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<BillPay> BillPays { get; set; }
        public DbSet<Payee> Payees { get; set; }
    }
}
