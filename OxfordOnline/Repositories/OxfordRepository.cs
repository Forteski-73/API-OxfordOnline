using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Repositories
{
    public class OxfordRepository : IOxfordRepository
    {
        private readonly AppDbContext _context;

        public OxfordRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Oxford?> GetByProductIdAsync(string productId) =>
            await _context.Oxford.FirstOrDefaultAsync(o => o.ProductId == productId);

        public async Task<List<Oxford>> GetByProductListIdsAsync(List<string> productIds) =>
            await _context.Oxford
                .Where(o => productIds.Contains(o.ProductId))
                .ToListAsync();

        public async Task AddAsync(Oxford oxford) =>
            await _context.Oxford.AddAsync(oxford);

        public async Task UpdateAsync(Oxford oxford)
        {
            _context.Entry(oxford).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Oxford oxford) =>
            _context.Oxford.Remove(oxford);

        public async Task SaveAsync() =>
            await _context.SaveChangesAsync();

        public async Task<List<ProductData>> FilterOxfordByFields(OxfordFilterRequest filter)
        {
            var oxfordList = await _context.Oxford
                .Where(o =>
                    (filter.ProductId       == null || filter.ProductId.Count       == 0 || filter.ProductId.       Contains(o.ProductId))     &&
                    (filter.FamilyId        == null || filter.FamilyId.Count        == 0 || filter.FamilyId.        Contains(o.FamilyId))      &&
                    (filter.BrandId         == null || filter.BrandId.Count         == 0 || filter.BrandId.         Contains(o.BrandId))       &&
                    (filter.DecorationId    == null || filter.DecorationId.Count    == 0 || filter.DecorationId.    Contains(o.DecorationId))  &&
                    (filter.TypeId          == null || filter.TypeId.Count          == 0 || filter.TypeId.          Contains(o.TypeId))        &&
                    (filter.ProcessId       == null || filter.ProcessId.Count       == 0 || filter.ProcessId.       Contains(o.ProcessId))     &&
                    (filter.SituationId     == null || filter.SituationId.Count     == 0 || filter.SituationId.     Contains(o.SituationId))   &&
                    (filter.LineId          == null || filter.LineId.Count          == 0 || filter.LineId.          Contains(o.LineId))        &&
                    (filter.QualityId       == null || filter.QualityId.Count       == 0 || filter.QualityId.       Contains(o.QualityId))     &&
                    (filter.BaseProductId   == null || filter.BaseProductId.Count   == 0 || filter.BaseProductId.   Contains(o.BaseProductId)) &&
                    (filter.ProductGroupId  == null || filter.ProductGroupId.Count  == 0 || filter.ProductGroupId.  Contains(o.ProductGroupId))
                )
                .ToListAsync();

            var productIds = oxfordList.Select(o => o.ProductId).Distinct().ToList();

            var products = await _context.Product
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId);

            var invents = await _context.Invent
                .Where(i => productIds.Contains(i.ProductId))
                .ToDictionaryAsync(i => i.ProductId);

            var taxInfos = await _context.TaxInformation
                .Where(t => productIds.Contains(t.ProductId))
                .ToDictionaryAsync(t => t.ProductId);

            return oxfordList.Select(ox => new ProductData
            {
                Product = products.GetValueOrDefault(ox.ProductId),
                Oxford = ox,
                Invent = invents.GetValueOrDefault(ox.ProductId),
                TaxInformation = taxInfos.GetValueOrDefault(ox.ProductId)
            }).ToList();
        }
    }
}
