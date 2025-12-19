using OxfordOnline.Models;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Services
{
    public class InventService
    {
        private readonly IInventRepository _invent;

        public InventService(IInventRepository invent)
        {
            _invent = invent;
        }

        public async Task<Invent?> GetInventByProductIdAsync(string productId) =>
            await _invent.GetByProductIdAsync(productId);

        public async Task<List<Invent>> GetInventListByProductIdsAsync(List<string> productIds) =>
            await _invent.GetByProductListIdsAsync(productIds);

        public async Task<bool> CreateOrUpdateInventsAsync(List<Invent> invents)
        {
            foreach (var inv in invents)
            {
                var existing = await _invent.GetByProductIdAsync(inv.ProductId);
                if (existing != null)
                {
                    existing.NetWeight      = inv.NetWeight;
                    existing.TaraWeight     = inv.TaraWeight;
                    existing.GrossWeight    = inv.GrossWeight;
                    existing.GrossDepth     = inv.GrossDepth;
                    existing.GrossWidth     = inv.GrossWidth;
                    existing.GrossHeight    = inv.GrossHeight;
                    existing.UnitVolume     = inv.UnitVolume;
                    existing.UnitVolumeML   = inv.UnitVolumeML;
                    existing.NrOfItems      = inv.NrOfItems;
                    existing.UnitId         = inv.UnitId;

                    await _invent.UpdateAsync(existing);
                }
                else
                {
                    await _invent.AddAsync(inv);
                }
            }

            await _invent.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateInventAsync(Invent invent)
        {
            await _invent.UpdateAsync(invent);
            await _invent.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteInventAsync(string productId)
        {
            var invent = await _invent.GetByProductIdAsync(productId);
            if (invent == null) return false;

            await _invent.DeleteAsync(invent);
            await _invent.SaveAsync();
            return true;
        }
    }
}
