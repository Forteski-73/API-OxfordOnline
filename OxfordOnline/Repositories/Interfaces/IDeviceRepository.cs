using OxfordOnline.Models;
using OxfordOnline.Models.Dto;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface IDeviceRepository
    {
        Task<List<DeviceUserDto>> GetDevicesWithInventoryGuidAsync();
        Task<Device?> UpdateAsync(int id, UpdateDeviceRequest request);
    }
}
