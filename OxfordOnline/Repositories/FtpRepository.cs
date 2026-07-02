using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OxfordOnline.Models.Dto;
using OxfordOnline.Repositories.Interfaces;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

// Nota: O FtpRepository irá usar o IFtpService que você já tem para a operação de download de baixo nível.

namespace OxfordOnline.Repositories
{
    public class FtpRepository : IFtpRepository
    {
        private readonly IFtpService _ftpService;
        private readonly ILogger<FtpRepository> _logger;
        private readonly FtpSettings _ftpSettings;

        private readonly string _ftpUser;
        private readonly string _ftpPassword;

        // Injeta o IFtpService
        public FtpRepository(IFtpService ftpService, ILogger<FtpRepository> logger, IOptions<FtpSettings> ftpOptions)
        {
            _ftpService = ftpService;
            _logger = logger;
            _ftpSettings = ftpOptions.Value;
        }

        public async Task<byte[]> DownloadFileBytesAsync(string remotePath)
        {
            try
            {
                // Chama o método existente do FtpService que retorna um Stream
                // (O FtpService implementado na sua primeira pergunta)
                using (var stream = await _ftpService.DownloadAsync(remotePath))
                {
                    // Converte o Stream retornado para array de bytes
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[FtpRepository] Falha ao baixar bytes do arquivo: {remotePath}");
                // Relança a exceção para ser tratada pela camada superior (Service)
                throw;
            }
        }

        public async Task UploadFileBytesAsync(string remotePath, byte[] fileBytes)
        {
            try
            {
                _logger.LogInformation($"[FtpRepository] remotePath ......................: {remotePath}");

                // Garante a existência do diretório via FtpService
                await _ftpService.EnsureFtpDirectoryExistsAsync(remotePath);

                var fullUrl = remotePath.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase)
                    ? remotePath
                    : $"ftp://{_ftpSettings.Host}/{remotePath.TrimStart('/')}";

                var request = (FtpWebRequest)WebRequest.Create(fullUrl);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(_ftpSettings.User, _ftpSettings.Password);
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = false;

                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    await requestStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                }

                using var response = (FtpWebResponse)await request.GetResponseAsync();
                _logger.LogInformation($"[FtpRepository] Upload concluído: {remotePath}. Status: {response.StatusDescription}");
            }
            catch (WebException ex)
            {
                _logger.LogError(ex, $"[FtpRepository] Erro FTP ao enviar arquivo: {remotePath}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[FtpRepository] Erro inesperado ao enviar arquivo: {remotePath}");
                throw;
            }
        }

        /// <summary>
        /// Exclui uma lista de arquivos do servidor FTP de forma assíncrona.
        /// </summary>
        /// <param name="remotePaths">Lista de caminhos remotos (paths) dos arquivos a serem excluídos.</param>
        /// <returns>Um Task que representa a operação assíncrona.</returns>
        public async Task DeleteFilesAsync(List<string> remotePaths)
        {
            if (remotePaths == null || remotePaths.Count == 0)
            {
                _logger.LogWarning("[FtpRepository] Tentativa de deletar arquivos com lista de paths vazia.");
                return;
            }

            // 1. Cria uma lista de Tasks para todas as operações de exclusão
            var deleteTasks = remotePaths.Select(remotePath => DeleteSingleFileAsync(remotePath)).ToList();

            // 2. Espera a conclusão de todas as Tasks
            await Task.WhenAll(deleteTasks);
        }

        /// <summary>
        /// Exclui um único arquivo do servidor FTP.
        /// Este método é interno para ser chamado dentro do FtpRepository.
        /// </summary>
        private async Task DeleteSingleFileAsync(string remotePath)
        {
            try
            {
                _logger.LogInformation($"*********************************[FtpRepository] Tentando deletar arquivo: {remotePath}");

                // ⭐️ Chama a operação de exclusão no seu IFtpService ⭐️
                await _ftpService.DeleteAsync(remotePath);

                _logger.LogInformation($"[FtpRepository] Arquivo deletado com sucesso: {remotePath}");
            }
            catch (Exception ex)
            {
                // Nota: Capturamos a exceção aqui para que a falha em um arquivo não
                // impeça a exclusão dos outros arquivos (Task.WhenAll).
                _logger.LogError(ex, $"********************************[FtpRepository] Falha ao deletar arquivo: {remotePath}");

                // Em um cenário de lista, geralmente logamos o erro, mas não
                // relançamos a exceção para evitar travar o processo batch.
                // Se a camada superior precisar de feedback sobre falhas,
                // o retorno do método precisaria ser alterado para List<FtpImageUploadResponse> ou similar.
            }
        }

    }
}