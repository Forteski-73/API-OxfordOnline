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

            // Tabela Product
            if (dtoInvent.Product != null && !string.IsNullOrEmpty(dtoInvent.Product.ProductId))
            {
                bool exists = await _context.Set<Product>().AnyAsync(p => p.ProductId == dtoInvent.Product.ProductId);
                if (!exists)
                {
                    await _context.Set<Product>().AddAsync(dtoInvent.Product);
                }
            }

            // Tabela Oxford
            if (dtoInvent.Oxford != null && !string.IsNullOrEmpty(dtoInvent.Oxford.ProductId))
            {
                bool exists = await _context.Set<Oxford>().AnyAsync(o => o.ProductId == dtoInvent.Oxford.ProductId);
                if (!exists)
                {
                    await _context.Set<Oxford>().AddAsync(dtoInvent.Oxford);
                }
            }

            // Tabela Invent
            if (dtoInvent.Invent != null && !string.IsNullOrEmpty(dtoInvent.Invent.ProductId))
            {
                bool exists = await _context.Invent.AnyAsync(i => i.ProductId == dtoInvent.Invent.ProductId);
                if (!exists)
                {
                    await _context.Invent.AddAsync(dtoInvent.Invent);
                }
            }

            // Tabela InventDim (Location)
            if (dtoInvent.Location != null && !string.IsNullOrEmpty(dtoInvent.Location.ProductId))
            {
                bool exists = await _context.Set<InventDim>().AnyAsync(l => l.ProductId == dtoInvent.Location.ProductId);
                if (!exists)
                {
                    await _context.Set<InventDim>().AddAsync(dtoInvent.Location);
                }
            }

            // Tabela TaxInformation
            if (dtoInvent.TaxInformation != null && !string.IsNullOrEmpty(dtoInvent.TaxInformation.ProductId))
            {
                bool exists = await _context.Set<TaxInformation>().AnyAsync(t => t.ProductId == dtoInvent.TaxInformation.ProductId);
                if (!exists)
                {
                    await _context.Set<TaxInformation>().AddAsync(dtoInvent.TaxInformation);
                }
            }

            // Se nenhum dos IFs acima disparar o AddAsync, o SaveChangesAsync não fará nada
            await _context.SaveChangesAsync();
        }
    }
}
