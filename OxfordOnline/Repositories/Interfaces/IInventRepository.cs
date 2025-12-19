using OxfordOnline.Models;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface IInventRepository
    {
        Task<Invent?> GetByProductIdAsync(string productId);
        Task<List<Invent>> GetByProductListIdsAsync(List<string> productIds);
        Task AddAsync(Invent invent);
        Task UpdateAsync(Invent invent);
        Task DeleteAsync(Invent invent);
        Task SaveAsync();
    }
}
