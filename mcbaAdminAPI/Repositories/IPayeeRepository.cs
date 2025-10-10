using mcbaMVC.Models;

namespace mcbaAdminAPI.Repositories
{
    public interface IPayeeRepository
    {
        Task<IEnumerable<Payee>> GetAllAsync();
        Task<IEnumerable<Payee>> GetByPostcodeAsync(string postcode);
        Task<Payee?> GetByIdAsync(int id);
        Task UpdateAsync(Payee payee);
    }
}
