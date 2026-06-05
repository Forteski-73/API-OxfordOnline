using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
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

        /// Recebe o DTO do cadastro de produto completo e grava todas as entidades preenchidas no banco de dados.
        public async Task SaveProductCompleteAsync(ProductComplete dtoInvent)
        {
            if (dtoInvent == null) throw new ArgumentNullException(nameof(dtoInvent), "Json não pode ser nulo.");

            if (dtoInvent.Product != null)        await _context.Set<Product>().AddAsync(dtoInvent.Product);
            if (dtoInvent.Oxford != null)         await _context.Set<Oxford>().AddAsync(dtoInvent.Oxford);
            if (dtoInvent.Invent != null)         await _context.Invent.AddAsync(dtoInvent.Invent);
            if (dtoInvent.Location != null)       await _context.Set<InventDim>().AddAsync(dtoInvent.Location);
            if (dtoInvent.TaxInformation != null) await _context.Set<TaxInformation>().AddAsync(dtoInvent.TaxInformation);

            // Persiste todas as alterações no banco de dados de uma vez só
            await _context.SaveChangesAsync();
        }
    }
}
