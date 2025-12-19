using OxfordOnline.Models;
using OxfordOnline.Models.Enums;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface IImageRepository
    {
        Task<Image?> GetByIdAsync(int id);
        Task<IEnumerable<Image>> GetByProductIdAsync(string productId, Finalidade finalidade, bool main);
        Task<List<Image>> GetByProductAsync(string productId, Finalidade finalidade);
        Task<IEnumerable<Image>> GetByProductIdsAsync(List<string> productIds);
        Task AddRangeAsync(IEnumerable<Image> images);
        Task RemoveByProductIdsAsync(IEnumerable<string> productIds); // atualizado
        Task SaveAsync();

        Task AddOrUpdateAsync(Image image);

        Task UpdateImagesByProductIdAsync(string productId, Finalidade finalidade, List<IFormFile> files);

        Task UpdateImagesByteAsync(string productId, Finalidade finalidade, List<byte[]> imageBytesList);

        //Task DeleteImagesByProductIdAsync(string productId);
        //Task SaveImageAsync(string productId, string fileName, Stream content);
        Task<Stream> DownloadFileStreamFromFtpAsync(string ftpFilePath);
    }
}
