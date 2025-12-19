using OxfordOnline.Models;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface ITaxInformationRepository
    {
        Task<TaxInformation?> GetByProductIdAsync(string productId);
        Task<List<TaxInformation>> GetByProductListIdsAsync(List<string> productIds);
        Task AddAsync(TaxInformation invent);
        Task UpdateAsync(TaxInformation invent);
        Task DeleteAsync(TaxInformation invent);
        Task SaveAsync();
    }
}
