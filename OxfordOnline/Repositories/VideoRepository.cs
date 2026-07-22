using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Repositories
{
    public class VideoRepository : IVideoRepository
    {
        private readonly AppDbContext _context;

        public VideoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VideoData>> GetActiveVideosAsync()
        {
            var query =
                from v in _context.Videos
                join c in _context.VideoCategories on v.CategoryId equals c.Id into cj
                from c in cj.DefaultIfEmpty()
                where v.Active
                orderby (c != null ? c.DisplayOrder : 0), v.DisplayOrder
                select new VideoData
                {
                    Id = v.Id,
                    Title = v.Title,
                    Description = v.Description,
                    VideoUrl = v.VideoUrl,
                    ThumbnailUrl = v.ThumbnailUrl,
                    DurationSeconds = v.DurationSeconds,
                    VideoOrder = v.DisplayOrder,
                    CategoryName = c != null ? c.Name : null,
                    CategoryColor = c != null ? c.ColorHex : null,
                    CategoryIcon = c != null ? c.IconName : null
                };

            return await query.ToListAsync();
        }

        public async Task<VideoData?> GetActiveVideoByIdAsync(int id)
        {
            var query =
                from v in _context.Videos
                join c in _context.VideoCategories on v.CategoryId equals c.Id into cj
                from c in cj.DefaultIfEmpty()
                where v.Active && v.Id == id
                select new VideoData
                {
                    Id = v.Id,
                    Title = v.Title,
                    Description = v.Description,
                    VideoUrl = v.VideoUrl,
                    ThumbnailUrl = v.ThumbnailUrl,
                    DurationSeconds = v.DurationSeconds,
                    VideoOrder = v.DisplayOrder,
                    CategoryName = c != null ? c.Name : null,
                    CategoryColor = c != null ? c.ColorHex : null,
                    CategoryIcon = c != null ? c.IconName : null
                };

            return await query.FirstOrDefaultAsync();
        }
    }
}