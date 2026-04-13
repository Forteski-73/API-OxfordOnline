using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Models.Enums;
using OxfordOnline.Resources;
using OxfordOnline.Services;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.Json;
using System.IO.Compression;

namespace OxfordOnline.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ImageService _imageService;
        private readonly ILogger<ImageController> _logger;

        public ImageController(ImageService imageService, ILogger<ImageController> logger)
        {
            _imageService = imageService;
            _logger = logger;
        }

        // POST: /Image
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateImages([FromBody] List<Image> images)
        {
            if (images == null || !images.Any())
                return BadRequest(new { message = EndPointsMessages.NoImagesProvided });

            if (images.Any(i => string.IsNullOrWhiteSpace(i.ProductId) || string.IsNullOrWhiteSpace(i.ImagePath)))
                return BadRequest(new { message = EndPointsMessages.InvalidImageField });

            try
            {
                var result = await _imageService.CreateOrReplaceImagesAsync(images);
                return Ok(new
                {
                    message = string.Format(EndPointsMessages.ImageSavedSuccess, result.Count()),
                    imagens = result
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, EndPointsMessages.LogErrorSavingImage);
                return StatusCode(500, new
                {
                    message = EndPointsMessages.ErrorSavingImages,
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // GET: /Image/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Image>> GetImageById(int id)
        {
            var image = await _imageService.GetImageByIdAsync(id);
            if (image == null)
                return NotFound(new { message = EndPointsMessages.ImageNotFound });

            return Ok(image);
        }

        // GET: /Image/Product/{productId}
        [Authorize]
        [HttpGet("Product/{productId}/{finalidade}")]
        public async Task<ActionResult<IEnumerable<Image>>> GetImagesByProductId(string productId, Finalidade finalidade)
        {
            var images = await _imageService.GetImagesByProductIdAsync(productId, finalidade, false);
            if (!images.Any())
                return NotFound(new { message = EndPointsMessages.ImageNotFoundForProduct });

            return Ok(images);
        }

        [Authorize]
        [HttpGet("ProductImage/{productId}/{finalidade}/{main}")]
        public async Task<IActionResult> DownloadZipByProduct(string productId, Finalidade finalidade, bool main = false)
        {
            if (string.IsNullOrWhiteSpace(productId))
                return BadRequest("Produto inválido.");

            try
            {
                var images = await _imageService.GetImagesByProductIdAsync(productId, finalidade, main);

                if (images == null || !images.Any())
                    return NotFound("Nenhuma imagem encontrada para o produto.");

                var zipStream = new MemoryStream(); // NÃO usar 'using' aqui

                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    // APENAS ADICIONA AS IMAGENS
                    foreach (var img in images)
                    {
                        if (string.IsNullOrWhiteSpace(img.ImagePath))
                            continue;

                        var ftpRelativePath = img.ImagePath.TrimStart('/').Replace('\\', '/');
                        var fileName = Path.GetFileName(ftpRelativePath);

                        try
                        {
                            var stream = await _imageService.DownloadImageStreamAsync(ftpRelativePath);
                            var entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);

                            using var entryStream = entry.Open();
                            await stream.CopyToAsync(entryStream);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"*** Erro ao adicionar imagem {fileName} no zip ***");
                        }
                    }
                }

                zipStream.Seek(0, SeekOrigin.Begin); // Reposiciona antes de retornar

                return File(zipStream, "application/zip", $"produto_{productId}_imagens.zip");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "*** Erro ao gerar zip de imagens do produto ***");
                return StatusCode(500, "Erro interno ao gerar imagens do produto.");
            }
        }

        [HttpPost("ReplaceProductImages/{productId}/{finalidade}")]
        public async Task<IActionResult> ReplaceImages(string productId, Finalidade finalidade, [FromForm] List<IFormFile> files)
        {
            if (string.IsNullOrWhiteSpace(productId))
                return BadRequest("Produto inválido.");

            if (files == null || !files.Any())
                return BadRequest("Nenhuma imagem enviada.");

            try
            {
                await _imageService.UpdateImagesByProductIdAsync(productId, finalidade, files);

                return Ok("Imagens substituídas com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "*** Erro ao substituir imagens ***");
                return StatusCode(500, "Erro ao salvar novas imagens.");
            }
        }

        [HttpPost("UpdateProductImages/{productId}/{finalidade}")]
        public async Task<IActionResult> UpdateImages(string productId, Finalidade finalidade, [FromForm] List<IFormFile> files)
        {
            _logger.LogError("       ********************* Chegou no UpdateProductImages *********************");
            if (string.IsNullOrWhiteSpace(productId))
                return BadRequest("Produto inválido.");
            _logger.LogError("       ********************* Chegou no UpdateProductImages 1 *********************");
            if (files == null || !files.Any())
                return BadRequest("Nenhuma imagem enviada.");

            _logger.LogError("       ********************* Chegou no UpdateProductImages 2 *********************");
            try
            {
                await _imageService.UpdateImagesByProductIdAsync(productId, finalidade, files);

                _logger.LogError("       ********************* Chegou no UpdateProductImages  Imagens substituídas com sucesso. *********************");
                return Ok("Imagens substituídas com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "*** Erro ao substituir imagens ***");
                return StatusCode(500, "Erro ao salvar novas imagens..");
            }
        }

        [HttpPost("ReplaceProductImages/Base64")]
        public async Task<IActionResult> ReplaceImagesBase64([FromBody] ProductImageBase64 request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProductId))
            {
                return BadRequest("Dados de requisição inválidos.");
            }

            if (request.Base64Images == null || !request.Base64Images.Any())
            {
                await _imageService.DeleteAllImagesByProductIdAsync(request.ProductId);
                return Ok("Imagens do produto excluídas com sucesso via Base64.");
                //return BadRequest("Nenhuma imagem em base64 enviada.");
            }

            try
            {
                var imageBytesList = new List<byte[]>();
                foreach (var base64Image in request.Base64Images)
                {
                    // Converte a string Base64 para um array de bytes
                    byte[] zippedBytes = Convert.FromBase64String(base64Image);

                    // Descompacta os bytes para obter o conteúdo original da imagem
                    using (var zippedStream = new MemoryStream(zippedBytes))
                    using (var archive = new ZipArchive(zippedStream, ZipArchiveMode.Read))
                    {
                        // Acha a primeira entrada (o arquivo de imagem) dentro do ZIP
                        var entry = archive.Entries.FirstOrDefault();
                        if (entry == null)
                        {
                            return BadRequest("O arquivo compactado não contém uma imagem válida.");
                        }

                        // Lê o conteúdo da imagem descompactada para um array de bytes
                        using (var entryStream = entry.Open())
                        using (var unzippedStream = new MemoryStream())
                        {
                            await entryStream.CopyToAsync(unzippedStream);
                            imageBytesList.Add(unzippedStream.ToArray());
                        }
                    }
                }

                // Chama o serviço com a nova lista de bytes das imagens descompactadas
                await _imageService.UpdateImagesByteAsync(request.ProductId, request.Finalidade, imageBytesList);

                return Ok("Imagens substituídas com sucesso via Base64.");
            }
            catch (FormatException)
            {
                return BadRequest("Uma ou mais strings de imagem não estão em um formato Base64 válido.");
            }
            catch (InvalidDataException) // Captura exceção específica de dados de ZIP inválidos
            {
                return BadRequest("O conteúdo Base64 não é um arquivo ZIP válido.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "*** Erro ao substituir imagens Base64 ***");
                return StatusCode(500, "Erro ao salvar novas imagens...");
            }
        }


        /* ************************************************************       GENERICO             **********************************************************/

        [HttpPost("UpdateImages/Base64")]
        public async Task<IActionResult> UpdateImagesBase64([FromBody] ImageGenBase64 request)
        {
            // 1. Validação Básica
            if (request == null || string.IsNullOrWhiteSpace(request.CodeId))
            {
                return BadRequest("Identificador é obrigatório.");
            }

            // 2. Cenário de Deleção: Se a lista estiver vazia, remove todas as imagens vinculadas ao ID
            if (request.Base64Images == null || !request.Base64Images.Any())
            {
                // Aqui você chama o método de deleção genérico do seu service
                await _imageService.DeleteAllImagesByPackIdAsync(request.CodeId);
                return Ok($"Todas as imagens associadas ao ID {request.CodeId} foram removidas.");
            }

            try
            {
                var imageBytesList = new List<byte[]>();

                // 3. Processamento das strings Base64 (ZIPs)
                foreach (var base64Zip in request.Base64Images)
                {
                    if (string.IsNullOrWhiteSpace(base64Zip)) continue;

                    // Converte Base64 para Bytes (ZIP)
                    byte[] zippedBytes = Convert.FromBase64String(base64Zip);

                    using (var zippedStream = new MemoryStream(zippedBytes))
                    using (var archive = new ZipArchive(zippedStream, ZipArchiveMode.Read))
                    {
                        // Processa cada arquivo dentro do ZIP (caso o ZIP tenha mais de uma imagem)
                        foreach (var entry in archive.Entries)
                        {
                            if (string.IsNullOrEmpty(entry.Name)) continue;

                            using (var entryStream = entry.Open())
                            using (var unzippedStream = new MemoryStream())
                            {
                                await entryStream.CopyToAsync(unzippedStream);
                                imageBytesList.Add(unzippedStream.ToArray());
                            }
                        }
                    }
                }

                if (!imageBytesList.Any())
                {
                    return BadRequest("Nenhum arquivo de imagem válido foi encontrado nos pacotes enviados.");
                }

                // 4. Persistência
                // Nota: Certifique-se de que o seu serviço aceite o CodeId genérico.
                // Se houver uma finalidade fixa ou padrão para este endpoint, passe-a aqui.
                await _imageService.UpdateImagesPackAsync(request.CodeId, request.CreatedUser, imageBytesList);

                return Ok(new
                {
                    Message = "Imagens atualizadas com sucesso.",
                    Count = imageBytesList.Count
                });
            }
            catch (FormatException)
            {
                return BadRequest("Formato Base64 inválido detectado.");
            }
            catch (InvalidDataException)
            {
                return BadRequest("Um ou mais pacotes enviados não são arquivos ZIP válidos.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"*** Erro ao processar UpdateImages para o ID: {request.CodeId} ***");
                return StatusCode(500, "Ocorreu um erro interno ao processar as imagens.");
            }
        }

    }
}