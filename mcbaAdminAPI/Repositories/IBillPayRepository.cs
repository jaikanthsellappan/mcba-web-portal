using mcbaMVC.Models;

namespace mcbaAdminAPI.Repositories
{
    public interface IBillPayRepository
    {
        Task<IEnumerable<BillPay>> GetAllAsync();
        Task<BillPay?> GetByIdAsync(int id);
        Task BlockAsync(int id);
        Task UnblockAsync(int id);
    }
}
