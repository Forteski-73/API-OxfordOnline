using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Repositories
{
    public class TvDeviceRepository : ITvDeviceRepository
    {
        private readonly AppDbContext _context;

        public TvDeviceRepository(AppDbContext context)
        {
            _context = context;
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
            return await _context.Device.AnyAsync(d => d.DeviceId == deviceId);
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
    }
}