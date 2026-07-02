using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Repositories
{
    public class TaxInformationRepository : ITaxInformationRepository
    {
        private readonly AppDbContext _context;

        public TaxInformationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaxInformation?> GetByProductIdAsync(string productId) =>
            await _context.TaxInformation.FirstOrDefaultAsync(t => t.ProductId == productId);

        public async Task<List<TaxInformation>> GetByProductListIdsAsync(List<string> productIds) =>
            await _context.TaxInformation
                .Where(t => productIds.Contains(t.ProductId))
                .ToListAsync();

        public async Task AddAsync(TaxInformation taxInfo) =>
            await _context.TaxInformation.AddAsync(taxInfo);

        public async Task UpdateAsync(TaxInformation taxInfo)
        {
            _context.Entry(taxInfo).State = EntityState.Modified;
        }

        public async Task DeleteAsync(TaxInformation taxInfo) =>
            _context.TaxInformation.Remove(taxInfo);

        public async Task SaveAsync() =>
            await _context.SaveChangesAsync();
    }
}
