using mcbaMVC.Data;
using mcbaMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace mcbaAdminAPI.Repositories
{
    public class BillPayRepository : IBillPayRepository
    {
        private readonly MCBAContext _context;
        public BillPayRepository(MCBAContext context) => _context = context;

        public async Task<IEnumerable<BillPay>> GetAllAsync()
{
    // Force EF to always read from the database, not memory
    _context.ChangeTracker.Clear(); 

    return await _context.BillPays
        .Include(b => b.Payee)
        .AsNoTracking()  // no caching or stale entity tracking
        .OrderBy(b => b.BillPayID)
        .ToListAsync();
}

        public async Task<BillPay?> GetByIdAsync(int id) =>
            await _context.BillPays.AsNoTracking().FirstOrDefaultAsync(b => b.BillPayID == id);

        public async Task BlockAsync(int id)
        {
            var bill = await _context.BillPays.FindAsync(id);
            if (bill != null)
            {
                bill.Status = "B";
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnblockAsync(int id)
        {
            var bill = await _context.BillPays.FindAsync(id);
            if (bill != null)
            {
                bill.Status = "S";
                await _context.SaveChangesAsync();
            }
        }
    }
}
