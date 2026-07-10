using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Models.Enums;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Repositories
{
    public class TvDeviceRepository : ITvDeviceRepository
    {
        private readonly AppDbContext _context;
        private readonly IImageRepository _imageRepository;

        public TvDeviceRepository(
            AppDbContext context,
            IImageRepository imageRepository)
        {
            _context = context;
            _imageRepository = imageRepository;
        }

        public async Task<IEnumerable<TvDevice>> GetAllAsync()
        {
            return await _context.TvDevice
                .Include(t => t.Device)
                .ToListAsync();
        }

        public async Task<TvDevice?> GetByDeviceIdAsync(int deviceId)
        {
            return await _context.TvDevice
                .Include(t => t.Device)
                .FirstOrDefaultAsync(t => t.DeviceId == deviceId);
        }

        public async Task<IEnumerable<TvDevice>> GetBySetorAsync(string setor)
        {
            return await _context.TvDevice
                .Include(t => t.Device)
                .Where(t => t.Setor == setor)
                .ToListAsync();
        }

        public async Task<bool> DeviceExistsAsync(int deviceId)
        {
            return await _context.Device.AnyAsync(d => d.Id == deviceId);
        }

        public async Task<TvDevice> AddAsync(TvDevice tvDevice)
        {
            _context.TvDevice.Add(tvDevice);
            await _context.SaveChangesAsync();
            return tvDevice;
        }

        public async Task<TvDevice?> UpdateAsync(int deviceId, string setor)
        {
            var existing = await _context.TvDevice.FindAsync(deviceId);
            if (existing == null)
                return null;

            existing.Setor = setor;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int deviceId)
        {
            var existing = await _context.TvDevice.FindAsync(deviceId);
            if (existing == null)
                return false;

            _context.TvDevice.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TvDevice> UpdateAsync(TvDevice tvDevice)
        {
            _context.TvDevice.Update(tvDevice);
            await _context.SaveChangesAsync();
            return tvDevice;
        }

        public async Task<TvDevice?> GetByGuidAndUserAsync(Guid guid, string user)
        {
            return await (
                from tv in _context.TvDevice
                join d in _context.Device
                    on tv.DeviceId equals d.Id
                join u in _context.ApiUser
                    on d.UserId equals u.Id
                where d.Guid == guid
                   && u.User == user
                select tv
            ).FirstOrDefaultAsync();
        }

        public async Task<List<Image>> GetByGuidAndUserIMGAsync(Guid guid, string user)
        {
            var transCode = await (
                from tv in _context.TvDevice
                join d in _context.Device on tv.DeviceId equals d.Id
                join u in _context.ApiUser on d.UserId equals u.Id
                where d.Guid == guid && u.User == user
                select tv.TransCode
            ).FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(transCode))
                return new List<Image>();

            return (await _imageRepository.GetByProductIdAsync(
                transCode,
                Finalidade.EMBALAGEM,
                false
            )).ToList();
        }
    }
}