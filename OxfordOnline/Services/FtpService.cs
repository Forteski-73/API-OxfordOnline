using Microsoft.Extensions.Configuration;
using OxfordOnline.Controllers;
using OxfordOnline.Repositories.Interfaces;
using OxfordOnline.Utils;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace OxfordOnline.Services
{
    public class FtpService : IFtpService
    {
        private readonly string _ftpHost;
        private readonly string _ftpUser;
        private readonly string _ftpPassword;
        private readonly IFtpService _ftpService;
        private readonly ILogger<ImageController> _logger;

        public FtpService(IConfiguration configuration, ILogger<ImageController> logger)
        {
            _ftpHost = configuration["Ftp:Host"] ?? throw new ArgumentNullException("Ftp:Host");
            _ftpUser = configuration["Ftp:User"] ?? throw new ArgumentNullException("Ftp:User");
            _ftpPassword = configuration["Ftp:Password"] ?? throw new ArgumentNullException("Ftp:Password");
            _logger = logger;
        }

        /*
        public async Task UploadAsync(string remotePath, Stream content)
        {
            var uri = new Uri($"ftp://{_ftpHost}/{remotePath}");
            var request = (FtpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(_ftpUser, _ftpPassword);
            request.UseBinary = true;
            request.UsePassive = true;
            request.KeepAlive = false;

            using var requestStream = await request.GetRequestStreamAsync();
            await content.CopyToAsync(requestStream);
            content.Position = 0; // Opcional, caso queira resetar posição após upload

            using var response = (FtpWebResponse)await request.GetResponseAsync();
            // Opcional: você pode validar response.StatusDescription
        }
        */

        public async Task UploadAsync(string remotePath, Stream content)
        {
            try
            {
                var uri = new Uri($"ftp://{_ftpHost}/{remotePath}");
                var request = (FtpWebRequest)WebRequest.Create(uri);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(_ftpUser, _ftpPassword);
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = false;

                _logger.LogInformation($"Iniciando upload para: {uri}");

                // Copia o conteúdo do stream para o stream da requisição
                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    await content.CopyToAsync(requestStream);
                }

                // Obtém a resposta do servidor FTP
                using (var response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    // Valida se o upload foi bem-sucedido. O status de "ClosingData"
                    // geralmente indica uma transferência de arquivo bem-sucedida.
                    if (response.StatusCode == FtpStatusCode.ClosingData)
                    {
                        _logger.LogInformation($"Upload concluído com sucesso para {remotePath}. Status: {response.StatusDescription}");
                    }
                    else
                    {
                        // Emite um aviso caso o status não seja o esperado.
                        _logger.LogWarning($"Upload para {remotePath} pode ter falhado. Status: {response.StatusDescription}");
                    }
                }
            }
            catch (WebException ex)
            {
                // Captura exceções específicas de rede ou FTP.
                var response = ex.Response as FtpWebResponse;
                if (response != null)
                {
                    _logger.LogError($"Erro no upload FTP para {remotePath}. Status: {response.StatusDescription}");
                }
                else
                {
                    _logger.LogError($"Erro de conexão ou genérico no upload para {remotePath}. Mensagem: {ex.Message}");
                }
                // Relança a exceção para o método chamador, permitindo que o problema seja tratado em um nível superior.
                throw;
            }
            catch (Exception ex)
            {
                // Captura outras exceções inesperadas.
                _logger.LogError(ex, $"Erro inesperado durante o upload para {remotePath}.");
                throw;
            }
        }

        public async Task DeleteAsync(string remotePath)
        {
            if (string.IsNullOrWhiteSpace(remotePath))
                throw new ArgumentException("Caminho remoto inválido.", nameof(remotePath));

            try
            {
                // Criação segura da URI
                //var uri = new Uri($"ftp://{_ftpHost}/{remotePath}");
                var uri = new Uri($"ftp://{_ftpUser}@{_ftpHost}/{remotePath}");
                if (await FileExistsAsync(uri))
                {
                    var request = (FtpWebRequest)WebRequest.Create(uri);
                    request.Method = WebRequestMethods.Ftp.DeleteFile;
                    request.Credentials = new NetworkCredential(_ftpUser, _ftpPassword);
                    request.UsePassive = true;
                    request.KeepAlive = false;

                    using var response = (FtpWebResponse)await request.GetResponseAsync();

                    if (response.StatusCode != FtpStatusCode.FileActionOK &&
                        response.StatusCode != FtpStatusCode.CommandOK)
                    {
                        throw new Exception($"Falha ao excluir o arquivo: {response.StatusDescription}");
                    }
                }
            }
            catch (WebException ex) when (ex.Response is FtpWebResponse ftpResponse)
            {
                // erros do FTP
                var status = ftpResponse.StatusDescription;
                throw new Exception($"Erro FTP ao excluir arquivo '{remotePath}': {status}", ex);
            }
            catch (Exception ex)
            {
                // erros gerais
                throw new Exception($"Erro inesperado ao excluir arquivo '{remotePath}': {ex.Message}", ex);
            }
        }
        private async Task<bool> FileExistsAsync(Uri uri)
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create(uri);
                //request.Method = WebRequestMethods.Ftp.GetFileSize;
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(_ftpUser, _ftpPassword);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;

                using var response = (FtpWebResponse)await request.GetResponseAsync();
                return true;
            }
            catch (WebException ex)
            {
                // Captura o erro específico para arquivo não encontrado.
                if (ex.Response is FtpWebResponse ftpResponse &&
                    (ftpResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable ||
                     ftpResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailableOrBusy))
                {
                    return false;
                }
                // Outros erros ainda devem ser lançados.
                throw;
            }
        }

        public async Task<Stream> DownloadAsync(string remotePath)
        {
            var uri = new Uri($"ftp://{_ftpHost}/{remotePath}");
            var request = (FtpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_ftpUser, _ftpPassword);
            request.UseBinary = true;
            request.UsePassive = true;
            request.KeepAlive = false;

            var response = (FtpWebResponse)await request.GetResponseAsync();
            return response.GetResponseStream();
        }

        public async Task EnsureDirectoryExistsAsync(FtpImagePathBuilder pathBuilder)
        {
            if (pathBuilder == null)
                return;

            var path = pathBuilder.BuildPath();

            if (string.IsNullOrWhiteSpace(path))
                return;

            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var currentPath = "";

            foreach (var part in parts)
            {
                currentPath += "/" + part;
                await CreateDirectoryIfNotExistsAsync(currentPath);
            }
        }

        public async Task EnsureFtpDirectoryExistsAsync(string remotePath)
        {
            if (string.IsNullOrWhiteSpace(remotePath))
            {
                _logger.LogWarning("[FtpService] Caminho remoto vazio ao tentar garantir diretório.");
                return;
            }

            try
            {
                var directoryPath = Path.GetDirectoryName(remotePath)?.Replace("\\", "/");
                if (string.IsNullOrEmpty(directoryPath))
                    return;

                var parts = directoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                string currentPath = "";

                foreach (var part in parts)
                {
                    currentPath += "/" + part;
                    var uri = new Uri($"ftp://{_ftpHost}{currentPath}");

                    var request = (FtpWebRequest)WebRequest.Create(uri);
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    request.Credentials = new NetworkCredential(_ftpUser, _ftpPassword);
                    request.UsePassive = true;
                    request.KeepAlive = false;

                    try
                    {
                        using var response = (FtpWebResponse)await request.GetResponseAsync();
                        _logger.LogInformation($"[FtpService] Diretório criado: {currentPath}");
                    }
                    catch (WebException ex)
                    {
                        if (ex.Response is FtpWebResponse ftpResponse &&
                            ftpResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            // Diretório já existe → ignora
                            _logger.LogDebug($"[FtpService] Diretório já existe: {currentPath}");
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[FtpService] Falha ao garantir existência de diretório: {remotePath}");
                throw;
            }
        }

        public async Task CreateDirectoryIfNotExistsAsync(string path)
        {
            try
            {
                var uri = new Uri($"ftp://{_ftpHost}{path}");
                var request = (FtpWebRequest)WebRequest.Create(uri);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(_ftpUser, _ftpPassword);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;

                using var response = (FtpWebResponse)await request.GetResponseAsync();
            }
            catch (WebException ex)
            {
                if (ex.Response is FtpWebResponse response &&
                    response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    // Diretório já existe
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
