using System.Threading.Tasks;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface IFtpRepository
    {
        /// <summary>
        /// Baixa um arquivo do caminho remoto fornecido e retorna seu conteúdo como array de bytes.
        /// </summary>
        /// <param name="remotePath">O caminho remoto do arquivo.</param>
        /// <returns>O array de bytes do arquivo.</returns>
        Task<byte[]> DownloadFileBytesAsync(string remotePath);
        Task UploadFileBytesAsync(string remotePath, byte[] fileBytes);
        Task DeleteFilesAsync(List<string> remotePaths);
    }
}