using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Models.Enums;
using OxfordOnline.Repositories.Interfaces;
using OxfordOnline.Services;
using System.IO.Compression;

namespace OxfordOnline.Repositories
{
    public class ProductPackRepository : IProductPackRepository
    {
        private readonly AppDbContext _context;
        private readonly IImageRepository _imageRepository;
        private readonly IFtpService _ftpService;
        private readonly ILogger<ProductRepository> _logger;

        public ProductPackRepository(AppDbContext context, IImageRepository imageRepository, IFtpService ftpService, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
            _imageRepository = imageRepository;
            _ftpService = ftpService;
        }

        /*public async Task<IEnumerable<ProductPack>> GetAllAsync()
        {
            return await _context.ProductPack
                .AsNoTracking()
                .ToListAsync();
        }*/

        public async Task<IEnumerable<ProductPack>> GetAllAsync()
        {
            return await _context.ProductPack
                .Include(p => p.Images)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Product) // igual seu GetItemsByPackIdAsync
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ProductPack?> GetByIdAsync(int id)
        {
            return await _context.ProductPack
                .Include(p => p.Images)
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.PackId == id);
        }

        public async Task<IEnumerable<ProductPack>> GetPacksByProductIdAsync(string productId)
        {
            // Busca pacotes onde a lista de itens contém o ID do produto informado
            return await _context.ProductPack
                .Include(p => p.Images)
                .Include(p => p.Items)
                .Where(p => p.Items.Any(i => i.PackProductId == productId))
                .ToListAsync();
        }

        public async Task AddAsync(ProductPack pack)
        {
            await _context.ProductPack.AddAsync(pack);
        }

        public async Task UpdateAsync(ProductPack pack)
        {
            _context.ProductPack.Update(pack);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(ProductPack pack)
        {
            _context.ProductPack.Remove(pack);
            await Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<ProductPackImage>> GetImagesByPackIdAsync(int packId)
        {
            return await _context.ProductPackImage
                .Where(i => i.PackId == packId)
                .ToListAsync();
        }

        public async Task<ProductPackImage?> GetImageAsync(int packId, int sequence)
        {
            return await _context.ProductPackImage
                .FirstOrDefaultAsync(i => i.PackId == packId && i.PackSequence == sequence);
        }

        public async Task AddImageAsync(ProductPackImage image)
        {
            await _context.ProductPackImage.AddAsync(image);
        }

        public async Task DeleteImageAsync(ProductPackImage image)
        {
            _context.ProductPackImage.Remove(image);
            await Task.CompletedTask;
        }


        //public async Task<List<ImagePackBase64>> GetPackImagesAsBase64Async(int packId)
        public async Task<IEnumerable<ImagePackBase64>> GetPackImagesAsBase64Async(int packId)
        {
            {
                var resultList = new List<ImagePackBase64>();

                try
                {
                    // 1. Busca as referências das imagens no banco de dados
                    var images = await _context.ProductPackImage
                        .Where(i => i.PackId == packId)
                        .ToListAsync();

                    if (images == null || !images.Any())
                        return resultList;

                    foreach (var img in images)
                    {
                        if (string.IsNullOrWhiteSpace(img.PackImagePath))
                            continue;

                        // Limpa o path para o FTP
                        var ftpRelativePath = img.PackImagePath.TrimStart('/').Replace('\\', '/');
                        var fileName = Path.GetFileName(ftpRelativePath);

                        try
                        {
                            // 2. Download do arquivo via Stream
                            using var imageFileStream = await _imageRepository.DownloadFileStreamFromFtpAsync(ftpRelativePath);

                            if (imageFileStream == null) continue;

                            // 3. Processamento do ZIP em memória
                            using var zipStream = new MemoryStream();
                            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
                            {
                                var entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                                using var entryStream = entry.Open();
                                await imageFileStream.CopyToAsync(entryStream);
                            }

                            // 4. Conversão para Base64
                            var zipBytes = zipStream.ToArray();
                            var imageZipBase64 = Convert.ToBase64String(zipBytes);

                            resultList.Add(new ImagePackBase64
                            {
                                CodeId = packId.ToString(),
                                ImagePath = img.PackImagePath,
                                Sequence = img.PackSequence,
                                ImagesBase64 = imageZipBase64
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Erro ao processar imagem '{fileName}' do pack '{packId}'. FTP: {ftpRelativePath}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erro geral ao processar imagens do pack '{packId}'.");
                }

                return resultList;
            }
        }

        public async Task DeleteByPackIdAsync(int packId)
        {
            var images = await _context.ProductPackImage
                .Where(x => x.PackId == packId)
                .ToListAsync();

            if (images.Any())
            {
                _context.ProductPackImage.RemoveRange(images);
            }
        }

        // --- Métodos para Itens (product_pack_item) ---
        public async Task<IEnumerable<ProductPackItem>> GetItemsByPackIdAsync(int packId)
        {
            return await _context.ProductPackItem
                    .Include(i => i.Product) // <--- CRUCIAL: Carrega o product na navegação
                    .AsNoTracking()
                    .Where(i => i.PackId == packId)
                    .ToListAsync();
        }

        public async Task<ProductPackItem?> GetItemAsync(int packId, string sku)
        {
            // Busca pela chave composta: ID do Pacote + Código do Item (SKU)
            return await _context.ProductPackItem
                .FirstOrDefaultAsync(i => i.PackId == packId && i.PackProductId == sku);
        }

        public async Task AddItemAsync(ProductPackItem item)
        {
            var exists = await _context.ProductPackItem
                .AnyAsync(p => p.PackId == item.PackId
                            && p.PackProductId == item.PackProductId);

            if (exists)
                return; // já existe, não faz nada

            await _context.ProductPackItem.AddAsync(item);
        }

        public async Task DeleteItemAsync(ProductPackItem item)
        {
            _context.ProductPackItem.Remove(item);
            await Task.CompletedTask;
        }
    }
}