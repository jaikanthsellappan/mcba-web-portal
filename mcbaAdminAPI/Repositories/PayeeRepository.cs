using mcbaMVC.Data;
using mcbaMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace mcbaAdminAPI.Repositories
{
    public class PayeeRepository : IPayeeRepository
    {
        private readonly MCBAContext _context;
        public PayeeRepository(MCBAContext context) => _context = context;

        public async Task<IEnumerable<Payee>> GetAllAsync() => await _context.Payees.ToListAsync();

        public async Task<IEnumerable<Payee>> GetByPostcodeAsync(string postcode) =>
            await _context.Payees.Where(p => p.Postcode == postcode).ToListAsync();

        public async Task<Payee?> GetByIdAsync(int id) => await _context.Payees.FindAsync(id);

        public async Task UpdateAsync(Payee payee)
{
    var existing = await _context.Payees.FindAsync(payee.PayeeID);
    if (existing == null)
        throw new Exception("Payee not found.");

    // Update fields manually to avoid detached entity tracking issues
    existing.Name = payee.Name;
    existing.Address = payee.Address;
    existing.City = payee.City;
    existing.State = payee.State;
    existing.Postcode = payee.Postcode;
    existing.Phone = payee.Phone;

    await _context.SaveChangesAsync();
}

    }
}
