using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly AppDbContext _context;

        public DeviceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DeviceUserDto>> GetDevicesWithInventoryGuidAsync()
        {
            return await (
                from d in _context.Device
                join ig in _context.InventoryGuid
                    on d.Guid.ToString() equals ig.InventGuid
                join h in _context.InventoryHeader
                    on ig.InventHeaderId equals h.Id
                join au in _context.ApiUser
                    on d.UserId equals au.Id
                select new DeviceUserDto
                {
                    Id = d.Id,
                    Guid = d.Guid,
                    UserId = d.UserId,
                    Username = au.User,
                    DeviceName = d.DeviceName,
                    CustomDeviceName = d.CustomDeviceName,
                    Platform = d.Platform,
                    IsActive = d.IsActive,
                    Header = h
                }
            ).ToListAsync();
        }

        public async Task<Device?> UpdateAsync(int id, UpdateDeviceRequest request)
        {
            var device = await _context.Device.FindAsync(id);
            if (device == null)
                return null;

            device.CustomDeviceName = request.CustomDeviceName;
            device.IsActive = request.IsActive;

            if (request.InventHeaderId.HasValue)
            {
                var guidText = device.Guid.ToString();
                var inventoryGuid = await _context.InventoryGuid
                    .FirstOrDefaultAsync(ig => ig.InventGuid == guidText);

                if (inventoryGuid == null)
                    throw new KeyNotFoundException($"Nenhum InventoryGuid vinculado ao device '{device.Guid}'.");

                var headerExists = await _context.InventoryHeader
                    .AnyAsync(h => h.Id == request.InventHeaderId.Value);
                if (!headerExists)
                    throw new KeyNotFoundException($"O Header de inventário '{request.InventHeaderId.Value}' não foi encontrado em InventoryHeader.");

                inventoryGuid.InventHeaderId = request.InventHeaderId.Value;
            }

            await _context.SaveChangesAsync();
            return device;
        }
    }
}
