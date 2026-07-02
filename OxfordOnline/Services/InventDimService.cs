using OxfordOnline.Models;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Services
{
    public class InventDimService
    {
        private readonly IInventDimRepository _inventDim;

        public InventDimService(IInventDimRepository inventDim)
        {
            _inventDim = inventDim;
        }

        public async Task<InventDim?> GetInventDimAsync(string productId, string? locationId, string? companyId)
        {
            return await _inventDim.GetByKeysAsync(productId, locationId, companyId);
        }

        public async Task<List<InventDim>> GetInventDimsByProductIdAsync(string productId)
        {
            return await _inventDim.GetByProductIdAsync(productId);
        }

        public async Task<bool> CreateOrUpdateInventDimsAsync(List<InventDim> inventDims)
        {
            foreach (var invDim in inventDims)
            {
                var existing = await _inventDim.GetByKeysAsync(invDim.ProductId, invDim.LocationId, invDim.CompanyId);

                if (existing != null)
                {
                    existing.Quantity = invDim.Quantity;
                    existing.Price = invDim.Price;

                    await _inventDim.UpdateAsync(existing);
                }
                else
                {
                    await _inventDim.AddAsync(invDim);
                }
            }

            await _inventDim.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateInventDimAsync(InventDim inventDim)
        {
            await _inventDim.UpdateAsync(inventDim);
            await _inventDim.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteInventDimAsync(string productId, string? locationId, string? companyId)
        {
            var inventDim = await _inventDim.GetByKeysAsync(productId, locationId, companyId);
            if (inventDim == null) return false;

            await _inventDim.DeleteAsync(inventDim);
            await _inventDim.SaveAsync();
            return true;
        }
    }
}
