using OxfordOnline.Models;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface IInventDimRepository
    {
        Task<InventDim?> GetByKeysAsync(string productId, string? locationId, string? companyId);
        Task<List<InventDim>> GetByProductIdAsync(string productId);
        Task<InventDim?> GetInventDimByProductIdAsync(string productId);
        Task AddAsync(InventDim inventDim);
        Task UpdateAsync(InventDim inventDim);
        Task DeleteAsync(InventDim inventDim);
        Task SaveAsync();
    }
}
