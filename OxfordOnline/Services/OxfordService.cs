using Microsoft.EntityFrameworkCore;
using OxfordOnline.Models;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Services
{
    public class OxfordService
    {
        private readonly IOxfordRepository _oxford;

        public OxfordService(IOxfordRepository oxford)
        {
            _oxford = oxford;
        }

        public async Task<Oxford?> GetOxfordByProductIdAsync(string productId) =>
            await _oxford.GetByProductIdAsync(productId);

        public async Task<List<Oxford>> GetOxfordListByProductIdsAsync(List<string> productIds) =>
            await _oxford.GetByProductListIdsAsync(productIds);

        public async Task<bool> CreateOrUpdateOxfordsAsync(List<Oxford> oxfords)
        {
            foreach (var ox in oxfords)
            {
                var existing = await _oxford.GetByProductIdAsync(ox.ProductId);
                if (existing != null)
                {
                    existing.FamilyId = ox.FamilyId;
                    existing.FamilyDescription = ox.FamilyDescription;
                    existing.BrandId = ox.BrandId;
                    existing.BrandDescription = ox.BrandDescription;
                    existing.DecorationId = ox.DecorationId;
                    existing.DecorationDescription = ox.DecorationDescription;
                    existing.TypeId = ox.TypeId;
                    existing.TypeDescription = ox.TypeDescription;
                    existing.ProcessId = ox.ProcessId;
                    existing.ProcessDescription = ox.ProcessDescription;
                    existing.SituationId = ox.SituationId;
                    existing.SituationDescription = ox.SituationDescription;
                    existing.LineId = ox.LineId;
                    existing.LineDescription = ox.LineDescription;
                    existing.QualityId = ox.QualityId;
                    existing.QualityDescription = ox.QualityDescription;
                    existing.BaseProductId = ox.BaseProductId;
                    existing.BaseProductDescription = ox.BaseProductDescription;
                    existing.ProductGroupId = ox.ProductGroupId;
                    existing.ProductGroupDescription = ox.ProductGroupDescription;

                    await _oxford.UpdateAsync(existing);
                }
                else
                {
                    await _oxford.AddAsync(ox);
                }
            }

            await _oxford.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateOxfordAsync(Oxford oxford)
        {
            await _oxford.UpdateAsync(oxford);
            await _oxford.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteOxfordAsync(string productId)
        {
            var oxford = await _oxford.GetByProductIdAsync(productId);
            if (oxford == null) return false;

            await _oxford.DeleteAsync(oxford);
            await _oxford.SaveAsync();
            return true;
        }

        public async Task<List<ProductData>> GetFilterOxfordByFields(OxfordFilterRequest filterRequest)
        {
            return await _oxford.FilterOxfordByFields(filterRequest);
        }
    }
}
