using OxfordOnline.Models;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface ITvDeviceRepository
    {
        Task<IEnumerable<TvDevice>> GetAllAsync();
        Task<TvDevice?> GetByDeviceIdAsync(int deviceId);
        Task<IEnumerable<TvDevice>> GetBySetorAsync(string setor);
        Task<bool> DeviceExistsAsync(int deviceId);
        Task<TvDevice> AddAsync(TvDevice tvDevice);
        Task<TvDevice?> UpdateAsync(int deviceId, string setor);
        Task<bool> DeleteAsync(int deviceId);

        Task<TvDevice> UpdateAsync(TvDevice tvDevice);

        Task<TvDevice?> GetByGuidAndUserAsync(Guid guid, string user);

        Task<List<Image>> GetByGuidAndUserIMGAsync(Guid guid, string user);

    }
}