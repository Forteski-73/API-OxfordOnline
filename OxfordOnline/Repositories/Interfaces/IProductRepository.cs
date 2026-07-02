using OxfordOnline.Models;
using OxfordOnline.Models.Dto;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByProductIdAsync(string productId);
        Task<List<Product>> GetByProductListIdsAsync(List<string> productIds);
        Task<List<Product>> GetByStatusAndLocationAsync(bool status, string locationId);
        Task<List<ProductOxford>> GetProductOxfordAsync(ProductOxfordFilters filters);
        Task<List<ProductOxfordDetails>> GetFilteredOxfordProductDetailsAsync(List<string> products);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetSearchAsync(string? product, string? barcode, string? family, string? brand, string? line, string? decoration, string? nome);
        Task<IEnumerable<ProductComplete>> GetAppProductAsync(string? product);
        Task<ProductSearchResponse> GetAppSearchAsync(AppProductFilterRequest filterRequest);
        Task<Product?> GetFirstAsync();
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Product product);
        Task SaveAsync();
    }
}
