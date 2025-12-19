using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Repositories
{
    public class InventRepository : IInventRepository
    {
        private readonly AppDbContext _context;

        public InventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Invent?> GetByProductIdAsync(string productId) =>
            await _context.Invent.FirstOrDefaultAsync(i => i.ProductId == productId);

        public async Task<List<Invent>> GetByProductListIdsAsync(List<string> productIds) =>
            await _context.Invent
                .Where(i => productIds.Contains(i.ProductId))
                .ToListAsync();

        public async Task AddAsync(Invent invent) =>
            await _context.Invent.AddAsync(invent);

        public async Task UpdateAsync(Invent invent)
        {
            _context.Entry(invent).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Invent invent) =>
            _context.Invent.Remove(invent);

        public async Task SaveAsync() =>
            await _context.SaveChangesAsync();
    }
}
