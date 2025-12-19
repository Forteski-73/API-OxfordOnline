using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OxfordOnline.Controllers;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Models.Enums;
using OxfordOnline.Repositories.Interfaces;
using OxfordOnline.Services;
using OxfordOnline.Utils;
using System.Net;

namespace OxfordOnline.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly AppDbContext _context;
        private readonly IFtpService _ftpService;
        private readonly ILogger<ImageController> _logger;

        public ImageRepository(AppDbContext context, IFtpService ftpService, ILogger<ImageController> logger)
        {
            _context = context;
            _ftpService = ftpService;
            _logger = logger;
        }

        public async Task<Image?> GetByIdAsync(int id)
        {
            return await _context.Image.FindAsync(id);
        }

        public async Task<IEnumerable<Image>> GetByProductIdAsync(string productId, Finalidade finalidade, bool main)
        {
            var query = _context.Image.AsQueryable();

            query = query.Where(i => i.ProductId == productId);

            if (finalidade != Finalidade.TODOS)
            {
                query = query.Where(i => i.Finalidade == finalidade.ToString());
            }

            if (main)
            {
                // Se main == true, filtra apenas as imagens principais
                query = query.Where(i => i.ImageMain);
            }

            return await query
                .OrderBy(i => i.Sequence)
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<Image> images)
        {
            await _context.Image.AddRangeAsync(images);
        }

        public async Task RemoveByProductIdsAsync(IEnumerable<string> productIds)
        {
            var imagesToRemove = await _context.Image
                .Where(i => productIds.Contains(i.ProductId))
                .ToListAsync();

            if (imagesToRemove.Any())
                _context.Image.RemoveRange(imagesToRemove);
        }

        public async Task<IEnumerable<Image>> GetByProductIdsAsync(List<string> productIds)
        {
            return await _context.Image
                .Where(i => productIds.Contains(i.ProductId))
                .ToListAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task AddOrUpdateAsync(Image image)
        {
            var existing = await _context.Image
                .FirstOrDefaultAsync(i => i.ProductId == image.ProductId && i.ImagePath == image.ImagePath);

            if (existing == null)
            {
                await _context.Image.AddAsync(image);
            }
            else
            {
                // Atualiza campos necessários
                existing.Finalidade = image.Finalidade;
                existing.ImagePath = image.ImagePath;
                existing.Sequence = image.Sequence;
                existing.ImageMain = image.ImageMain;
                _context.Image.Update(existing);
            }
        }
        public async Task UpdateImagesByteAsync(string productId, Finalidade finalidade, List<byte[]> imageBytesList)
        {
            _logger.LogError("**** INICIO UpdateImagesByteAsync ****");
            try
            {
                var images = await _context.Image.Where(i => i.ProductId == productId && i.Finalidade == finalidade.ToString()).ToListAsync();

                string directoryPath;

                if (images.Any())
                {
                    // Pega o diretório da primeira imagem existente para usar como base
                    directoryPath = Path.GetDirectoryName(images.First().ImagePath)?.Replace("\\", "/") ?? string.Empty;

                    // Deleta as imagens antigas do FTP e do contexto
                    foreach (var image in images)
                    {
                        if (!string.IsNullOrEmpty(image.ImagePath))
                        {
                            await _ftpService.DeleteAsync(image.ImagePath);
                        }
                        _context.Image.Remove(image);
                    }
                }
                else
                {
                    // Se não houver imagens, cria um novo diretório no FTP
                    var oxford = await _context.Oxford.FirstOrDefaultAsync(o => o.ProductId == productId);
                    if (oxford == null)
                    {
                        _logger.LogError($"**** Produto não encontrado: {productId} ****");
                        throw new KeyNotFoundException($"Produto não encontrado: {productId}");
                    }

                    var pathBuilder = new FtpImagePathBuilder(
                        oxford.FamilyDescription.Replace(" ", "_"),
                        oxford.BrandDescription.Replace(" ", "_"),
                        oxford.LineDescription.Replace(" ", "_"),
                        oxford.DecorationDescription.Replace(" ", "_"),
                        oxford.ProductId,
                        finalidade.ToString()
                    );

                    // Garante que o diretório existe
                    await _ftpService.EnsureDirectoryExistsAsync(pathBuilder);
                    directoryPath = pathBuilder.ToString();
                }

                if (string.IsNullOrEmpty(directoryPath))
                {
                    throw new InvalidOperationException("Não foi possível determinar o diretório de destino.");
                }

                int seqImg = 1;
                bool mainImg = true;
                foreach (var imageBytes in imageBytesList) // Mudei de 'files' para 'imageBytesList'
                {
                    var formattedSequence = seqImg.ToString("D4");
                    var fileName = $"{formattedSequence}.jpeg";

                    using var stream = new MemoryStream(imageBytes);

                    var ftpPath = $"{directoryPath}/{fileName}";

                    _logger.LogError($"**** IMAGEM: {ftpPath} ****");
                    await _ftpService.UploadAsync(ftpPath, stream);

                    var imageNew = new Image
                    {
                        ProductId = productId,
                        ImagePath = ftpPath,
                        Finalidade = finalidade.ToString(),
                        Sequence = seqImg,
                        ImageMain = mainImg
                    };
                    _context.Image.Add(imageNew);

                    mainImg = false;
                    seqImg++;
                }

                await _context.SaveChangesAsync();
                _logger.LogError("**** IMAGENS ATUALIZADAS E SALVAS NO BANCO DE DADOS ****");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"**** Erro ao atualizar imagens para o produto: {productId} ****");
                throw;
            }
        }

        public async Task UpdateImagesByProductIdAsync(string productId, Finalidade finalidade, List<IFormFile> files)
        {
            _logger.LogError("**** INICIO UpdateImagesByProductIdAsync ****");
            try
            {
                var images = await _context.Image.Where(i => i.ProductId == productId && i.Finalidade == finalidade.ToString()).ToListAsync();

                string directoryPath;

                if (images.Any())
                {
                    // Pega o diretório da primeira imagem existente para usar como base
                    directoryPath = Path.GetDirectoryName(images.First().ImagePath)?.Replace("\\", "/") ?? string.Empty;

                    // Deleta as imagens antigas do FTP e do contexto
                    foreach (var image in images)
                    {
                        if (!string.IsNullOrEmpty(image.ImagePath))
                        {
                            await _ftpService.DeleteAsync(image.ImagePath);
                        }
                        _context.Image.Remove(image);
                    }
                }
                else
                {
                    // Se não houver imagens, cria um novo diretório no FTP
                    var oxford = await _context.Oxford.FirstOrDefaultAsync(o => o.ProductId == productId);
                    if (oxford == null)
                    {
                        _logger.LogError($"**** Produto não encontrado: {productId} ****");
                        throw new KeyNotFoundException($"Produto não encontrado: {productId}");
                    }

                    var pathBuilder = new FtpImagePathBuilder(
                        oxford.FamilyDescription.Replace(" ", "_"),
                        oxford.BrandDescription.Replace(" ", "_"),
                        oxford.LineDescription.Replace(" ", "_"),
                        oxford.DecorationDescription.Replace(" ", "_"),
                        oxford.ProductId,
                        finalidade.ToString()
                    );

                    // Garante que o diretório existe
                    await _ftpService.EnsureDirectoryExistsAsync(pathBuilder);
                    directoryPath = pathBuilder.ToString();
                }

                if (string.IsNullOrEmpty(directoryPath))
                {
                    throw new InvalidOperationException("Não foi possível determinar o diretório de destino.");
                }

                int seqImg = 1;
                bool mainImg = true;
                foreach (var file in files)
                {
                    if (file.Length == 0) continue;

                    using var stream = file.OpenReadStream();

                    var ftpPath = $"{directoryPath}/{file.FileName}";

                    _logger.LogError($"**** IMAGEM: {ftpPath} ****");
                    await _ftpService.UploadAsync(ftpPath, stream);

                    var imageNew = new Image
                    {
                        ProductId = productId,
                        ImagePath = ftpPath,
                        Finalidade = finalidade.ToString(),
                        Sequence = seqImg,
                        ImageMain = mainImg
                    };
                    _context.Image.Add(imageNew);

                    mainImg = false;
                    seqImg++;
                }

                await _context.SaveChangesAsync();
                _logger.LogError("**** IMAGENS ATUALIZADAS E SALVAS NO BANCO DE DADOS ****");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"**** Erro ao atualizar imagens para o produto: {productId} ****");
                throw;
            }
        }

        /*public async Task UpdateImagesByProductIdAsync(string productId, Finalidade finalidade, List<IFormFile> files)
        {
            _logger.LogError("**** INICIO  UpdateImagesByProductIdAsync ****");
            try
            {
                var images = await _context.Image.Where(i => i.ProductId == productId && i.Finalidade == finalidade.ToString()).ToListAsync();

                var firstImage = images.FirstOrDefault();

                if (firstImage == null)
                {
                    var oxford = await _context.Oxford.FirstOrDefaultAsync(o => o.ProductId == productId);
                    //family_description, brand_description, line_description, decoration_description
                    if (oxford == null)
                    {
                        _logger.LogError($"**** Produto não encontrado: {productId} ****");
                        throw new KeyNotFoundException($"Produto não encontrado: {productId}");
                    }

                    var pathBuilder = new FtpImagePathBuilder(
                        oxford.FamilyDescription.      Replace(" ", "_"),
                        oxford.BrandDescription.       Replace(" ", "_"),
                        oxford.LineDescription.        Replace(" ", "_"),
                        oxford.DecorationDescription.  Replace(" ", "_"),
                        oxford.ProductId,
                        finalidade.ToString()
                    );

                    var imagePath = $"{ pathBuilder.ToString() }/";

                    _logger.LogError($"**************** Verificando diretório no FTP: {imagePath} ****************");

                    await _ftpService.EnsureDirectoryExistsAsync(pathBuilder);

                    firstImage = new Image 
                    {
                        ProductId = productId,
                        ImagePath = imagePath,
                        Finalidade = finalidade.ToString(),
                    };
                }

                if (firstImage != null)
                {
                    _logger.LogError("************************** INICIO  FTP **************************");
                    foreach (var image in images)
                    {
                        if (!string.IsNullOrEmpty(image.ImagePath))
                        {
                            await _ftpService.DeleteAsync(image.ImagePath); // remove a imagem do FTP
                        }
                        _context.Image.Remove(image);  // remove a imagem do contexto
                    }

                    int seqImg = 1;
                    bool mainImg = true;
                    foreach (var file in files)
                    {
                        if (file.Length == 0) continue;

                        using var stream = file.OpenReadStream();

                        var baseDir = Path.GetDirectoryName(firstImage.ImagePath)?.Replace("\\", "/");
                        var ftpPath = $"{baseDir}/{file.FileName}";

                        await _ftpService.UploadAsync(ftpPath, stream);

                        _logger.LogError($"***************************************** Diretório no FTP: {ftpPath} *****************************************");

                        var imageNew = new Image
                        {
                            ProductId = productId,
                            ImagePath = ftpPath,
                            Finalidade = finalidade.ToString(),
                            Sequence = seqImg,
                            ImageMain = mainImg
                        };
                        _context.Image.Add(imageNew); // adiciona a nova imagem ao contexto

                        mainImg = false;
                        seqImg++;
                    }
                    _logger.LogError("**** ATUALIZOU DB ****");
                    await _context.SaveChangesAsync(); // atualiza o banco de dados

                }
                else
                {
                    _logger.LogError("**** Nenhuma imagem existente para basear o caminho. ****");
                    throw new InvalidOperationException("Nenhuma imagem existente para basear o caminho.");
                }
            }
            catch (Exception ex)
            {
                // Registra o erro com mais contexto
                _logger.LogError(ex, $"**** Erro ao atualizar imagens para o produto: {productId} ****");

                // Repropaga a exceção para ser tratada no controller (ou middleware)
                throw;
            }
        }
        */

        public async Task<Stream> DownloadFileStreamFromFtpAsync(string ftpFilePath)
        {
            return await _ftpService.DownloadAsync(ftpFilePath);
        }

        public async Task<List<Image>> GetByProductAsync(string productId, Finalidade finalidade)
        {
            var query = _context.Image
                .Where(i => i.ProductId == productId);

            if (finalidade != Finalidade.TODOS)
            {
                query = query.Where(i => i.Finalidade == finalidade.ToString());
            }

            return await query
                .OrderBy(i => i.Sequence)
                .ToListAsync();
        }
    }
}
