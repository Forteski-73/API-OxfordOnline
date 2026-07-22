using OxfordOnline.Models;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Services
{
    public class VideoService
    {
        private readonly IVideoRepository _video;

        public VideoService(IVideoRepository video)
        {
            _video = video;
        }

        public async Task<List<VideoData>> GetActiveVideosAsync() =>
            await _video.GetActiveVideosAsync();

        public async Task<VideoData?> GetActiveVideoByIdAsync(int id) =>
            await _video.GetActiveVideoByIdAsync(id);
    }
}