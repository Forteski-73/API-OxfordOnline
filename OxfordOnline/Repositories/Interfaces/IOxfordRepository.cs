using OxfordOnline.Models;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface IOxfordRepository
    {
        Task<Oxford?> GetByProductIdAsync(string productId);
        Task<List<Oxford>> GetByProductListIdsAsync(List<string> productIds);

        Task<List<ProductData>> FilterOxfordByFields(OxfordFilterRequest filterRequest);

        Task AddAsync(Oxford oxford);
        Task UpdateAsync(Oxford oxford);
        Task DeleteAsync(Oxford oxford);
        Task SaveAsync();
    }
}