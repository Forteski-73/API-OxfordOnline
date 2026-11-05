using OxfordOnline.Models;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface IBreakageRepository
    {
        Task<BreakageRecord?> GetByIdAsync(uint breakageId);
        Task<List<BreakageRecord>> GetCreatedSinceAsync(DateTime since);
        Task<bool> ProductExistsAsync(string productId);
        Task AddAsync(BreakageRecord breakage);
        Task DeleteAsync(BreakageRecord breakage);
        Task SaveAsync();
    }
}
