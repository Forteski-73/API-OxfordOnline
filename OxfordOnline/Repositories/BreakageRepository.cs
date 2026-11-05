using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Repositories
{
    public class BreakageRepository : IBreakageRepository
    {
        private readonly AppDbContext _context;

        public BreakageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BreakageRecord?> GetByIdAsync(uint breakageId) =>
            await _context.BreakageRecord.FirstOrDefaultAsync(b => b.BreakageId == breakageId);

        public async Task<List<BreakageRecord>> GetCreatedSinceAsync(DateTime since) =>
            await _context.BreakageRecord
                .AsNoTracking()
                .Where(b => b.CreatedAt >= since)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

        public async Task<bool> ProductExistsAsync(string productId) =>
            await _context.Product.AnyAsync(p => p.ProductId == productId);

        public async Task AddAsync(BreakageRecord breakage) =>
            await _context.BreakageRecord.AddAsync(breakage);

        public Task DeleteAsync(BreakageRecord breakage)
        {
            _context.BreakageRecord.Remove(breakage);
            return Task.CompletedTask;
        }

        public async Task SaveAsync() =>
            await _context.SaveChangesAsync();
    }
}
