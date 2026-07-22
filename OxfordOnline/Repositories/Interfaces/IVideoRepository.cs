using OxfordOnline.Models;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface IVideoRepository
    {
        Task<List<VideoData>> GetActiveVideosAsync();
        Task<VideoData?> GetActiveVideoByIdAsync(int id);
    }
}