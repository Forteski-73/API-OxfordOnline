using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OxfordOnline.Models.Dto;
using OxfordOnline.Repositories.Interfaces;
using OxfordOnline.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OxfordOnline.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class FtpController : ControllerBase
    {
        private readonly IFtpRepository _ftpRepository;
        private readonly ILogger<FtpController> _logger;

        // Construtor para injetar IFtpRepository
        public FtpController(IFtpRepository ftpRepository, ILogger<FtpController> logger)
        {
            _ftpRepository = ftpRepository;
            _logger = logger;
        }

        /// <summary>
        /// Recebe uma lista de caminhos de imagens do FTP, baixa e retorna o conteúdo em Base64.
        /// </summary>
        [Authorize]
        [HttpPost("Images/GetBase64")]
        [ProducesResponseType(typeof(List<FtpImageResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetImagesBase64([FromBody] FtpImageRequest request)
        {
            if (request == null || !request.ImageUrls.Any())
            {
                return BadRequest(new { message = EndPointsMessages.NoPath });
            }

            // Usa Task.WhenAll para processar downloads em paralelo para melhor performance
            var downloadTasks = request.ImageUrls
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(path => ProcessImageDownload(path));

            var results = await Task.WhenAll(downloadTasks);

            _logger.LogInformation($"Processamento de {results.Count()} imagens concluído. {results.Count(r => r.Status == "Error")} falhas.");

            // Retorna 200 OK mesmo que algumas imagens tenham falhado,
            // pois o status de sucesso/erro está detalhado no corpo da resposta.
            return Ok(results.ToList());
        }

        /// <summary>
        /// Tarefa auxiliar para baixar uma imagem e codificá-la em Base64.
        /// </summary>
        private async Task<FtpImageResponse> ProcessImageDownload(string path)
        {
            var response = new FtpImageResponse { Url = path };

            try
            {
                byte[] imageBytes = await _ftpRepository.DownloadFileBytesAsync(path);

                // Converte os bytes para Base64
                response.Base64Content = Convert.ToBase64String(imageBytes);
                response.Status = "Success";
            }
            catch (WebException ex) when (ex.Response is FtpWebResponse ftpResponse)
            {
                response.Status = "Error";
                response.Message = EndPointsMessages.ErrorFTP.Replace("%Error%", ftpResponse.StatusDescription);

                _logger.LogError(ex, $"Erro FTP ao processar imagem '{path}'. Status: {ftpResponse.StatusDescription}");
            }
            catch (Exception ex)
            {
                response.Status = "Error";
                response.Message = EndPointsMessages.Error.Replace("%Error%", ex.InnerException?.Message ?? ex.Message);

                _logger.LogError(ex, $"Erro inesperado ao processar imagem '{path}'.");
            }

            return response;
        }

        [Authorize]
        [HttpPost("Images/SetBase64")]
        [ProducesResponseType(typeof(List<FtpImageUploadResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SetImagesBase64([FromBody] FtpImageUploadRequest request)
        {
            if (request?.Images == null || !request.Images.Any())
                return BadRequest(new { message = EndPointsMessages.ImageNull });

            var uploadTasks = request.Images
                .Where(img => !string.IsNullOrWhiteSpace(img.Url) && !string.IsNullOrWhiteSpace(img.Base64Content))
                .Select(img => ProcessImageUpload(img));

            var results = await Task.WhenAll(uploadTasks);

            _logger.LogInformation($"Upload de {results.Length} imagens concluído. " +
                                   $"{results.Count(r => r.Status == "Error")} falhas.");

            return Ok(results.ToList());
        }

        private async Task<FtpImageUploadResponse> ProcessImageUpload(FtpImageUploadItem img)
        {
            var response = new FtpImageUploadResponse { Url = img.Url };

            try
            {
                // Decodifica o Base64 em bytes
                byte[] fileBytes = Convert.FromBase64String(img.Base64Content);

                // Envia o arquivo via repositório
                await _ftpRepository.UploadFileBytesAsync(img.Url, fileBytes);

                response.Status = "Success";
                response.Message = EndPointsMessages.Sucess;
            }
            catch (FormatException ex)
            {
                response.Status = "Error";
                response.Message = EndPointsMessages.InvalidFile;
                _logger.LogError(ex, $"Erro de Base64 ao processar '{img.Url}'.");
            }
            catch (WebException ex)
            {
                response.Status = "Error";
                response.Message = EndPointsMessages.ErrorFTP.Replace("%Error%", ex.Message);

                _logger.LogError(ex, $"Erro FTP ao enviar '{img.Url}'.");
            }
            catch (Exception ex)
            {
                response.Status = "Error";
                response.Message = EndPointsMessages.Error.Replace("%Error%", ex.InnerException?.Message ?? ex.Message);
                
                _logger.LogError(ex, $"Erro inesperado ao enviar '{img.Url}'.");
            }

            return response;
        }

        [Authorize]
        [HttpDelete("Images")] // Rota: DELETE v1/Ftp/Images
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteImages([FromBody] FtpImageRequest request)
        {
            if (request == null || !request.ImageUrls.Any())
            {
                return BadRequest(new { message = EndPointsMessages.NoPath });
            }

            // Filtra caminhos vazios ou nulos
            var pathsToDelete = request.ImageUrls
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            if (!pathsToDelete.Any())
            {
                return BadRequest(new { message = EndPointsMessages.NoPath });
            }

            _logger.LogInformation($"Iniciando exclusão de {pathsToDelete.Count} arquivos FTP.");

            try
            {
                // Chama o método do repositório para exclusão em lote (concorrente)
                // O FtpRepository.DeleteFilesAsync já trata falhas individuais e logs.
                await _ftpRepository.DeleteFilesAsync(pathsToDelete);

                return Ok(new { 
                    message = EndPointsMessages.Sucess
                });
            }
            catch (Exception ex)
            {
                var baseError = ex.InnerException?.Message ?? ex.Message;
                
                _logger.LogError(ex, $"Erro fatal ao iniciar a exclusão de imagens FTP.");

                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new { message = EndPointsMessages.Error.Replace("%Error%", baseError) });
            }
        }

    }
}