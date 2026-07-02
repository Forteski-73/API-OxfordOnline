using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Repositories
{
    public class InventDimRepository : IInventDimRepository
    {
        private readonly AppDbContext _context;

        public InventDimRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<InventDim?> GetByKeysAsync(string productId, string? locationId, string? companyId)
        {
            return await _context.InventDim.FirstOrDefaultAsync(i =>
                i.ProductId == productId &&
                i.LocationId == locationId &&
                i.CompanyId == companyId);
        }

        public async Task<List<InventDim>> GetByProductIdAsync(string productId)
        {
            return await _context.InventDim
                .Where(i => i.ProductId == productId)
                .ToListAsync();
        }

        public async Task<InventDim?> GetInventDimByProductIdAsync(string productId)
        {
            return await _context.InventDim
                .FirstOrDefaultAsync(id => id.ProductId == productId);
        }

        public async Task AddAsync(InventDim inventDim)
        {
            await _context.InventDim.AddAsync(inventDim);
        }

        public async Task UpdateAsync(InventDim inventDim)
        {
            _context.Entry(inventDim).State = EntityState.Modified;
        }

        public async Task DeleteAsync(InventDim inventDim)
        {
            _context.InventDim.Remove(inventDim);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
