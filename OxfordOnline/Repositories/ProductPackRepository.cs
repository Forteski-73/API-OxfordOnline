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

        public async Task<IEnumerable<PalletGroup>> GetAllAsync()
        {
            return await _context.ProductPack
                .Include(p => p.Images)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Product) // igual seu GetItemsByPackIdAsync
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PalletGroup?> GetByIdAsync(int id)
        {
            return await _context.ProductPack
                .Include(p => p.Images)
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.PackId == id);
        }

        public async Task<IEnumerable<PalletGroup>> GetPacksByProductIdAsync(string productId)
        {
            // Busca pacotes onde a lista de itens contém o ID do produto informado
            return await _context.ProductPack
                .Include(p => p.Images)
                //.Include(p => p.Items)
                .Where(p => p.Items.Any(i => i.PackProductId == productId))
                .ToListAsync();
        }

        public async Task AddAsync(PalletGroup pack)
        {
            await _context.ProductPack.AddAsync(pack);
        }

        public async Task UpdateAsync(PalletGroup pack)
        {
            _context.ProductPack.Update(pack);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(PalletGroup pack)
        {
            _context.ProductPack.Remove(pack);
            await Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<PalletGroupImage>> GetImagesByPackIdAsync(int packId)
        {
            return await _context.ProductPackImage
                .Where(i => i.PackId == packId)
                .ToListAsync();
        }

        public async Task<PalletGroupImage?> GetImageAsync(int packId, int sequence)
        {
            return await _context.ProductPackImage
                .FirstOrDefaultAsync(i => i.PackId == packId && i.PackSequence == sequence);
        }

        public async Task AddImageAsync(PalletGroupImage image)
        {
            await _context.ProductPackImage.AddAsync(image);
        }

        public async Task DeleteImageAsync(PalletGroupImage image)
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
        public async Task<IEnumerable<PalletGroupItem>> GetItemsByPackIdAsync(int packId)
        {
            return await _context.ProductPackItem
                    .Include(i => i.Product) // <--- CRUCIAL: Carrega o product na navegação
                    .AsNoTracking()
                    .Where(i => i.PackId == packId)
                    .ToListAsync();
        }

        public async Task<PalletGroupItem?> GetItemAsync(int packId, string sku)
        {
            // Busca pela chave composta: ID do Pacote + Código do Item (SKU)
            return await _context.ProductPackItem
                .FirstOrDefaultAsync(i => i.PackId == packId && i.PackProductId == sku);
        }

        public async Task<PalletGroupItem> AddItemAsync(PalletGroupItem item)
        {
            var existingItem = await _context.ProductPackItem
                .Include(p => p.Product)
                .FirstOrDefaultAsync(p => p.PackId == item.PackId
                                     && p.PackProductId == item.PackProductId);

            if (existingItem != null)
            {
                return existingItem;
            }

            await _context.ProductPackItem.AddAsync(item);

            // Força o EF Core a carregar a propriedade de navegação 'Product' 
            await _context.Entry(item).Reference(p => p.Product).LoadAsync();

            return item;
        }

        public async Task DeleteItemAsync(PalletGroupItem item)
        {
            _context.ProductPackItem.Remove(item);
            await Task.CompletedTask;
        }

        public async Task DeleteItemsByPackIdAsync(int packId)
        {
            var items = await _context.ProductPackItem
                .Where(x => x.PackId == packId)
                .ToListAsync();

            if (items.Any())
            {
                _context.ProductPackItem.RemoveRange(items);
            }
        }



        // --------------- Métodos para BOM (product_packing_bom) ---------------

        public async Task<IEnumerable<ProductPackingBom>> GetBomsByProductIdAsync(string productId)
        {
            return await _context.ProductPackingBom
                .AsNoTracking()
                .Where(b => b.ProductId == productId)
                .OrderBy(b => b.ProductSeq)
                .ToListAsync();
        }
        public async Task UpsertBomAsync(ProductPackingBomRequest request)
        {
            // 1. Busca e remove todos os registros existentes para este ProductId específico
            var existingBoms = await _context.ProductPackingBom
                .Where(b => b.ProductId == request.ProductId)
                .ToListAsync();

            if (existingBoms.Any())
            {
                _context.ProductPackingBom.RemoveRange(existingBoms);
            }

            // 2. Cria a lista com os novos registros mapeados a partir do Request
            var newBoms = request.BomItems.Select(item => new ProductPackingBom
            {
                ProductId = request.ProductId,
                ProductBomId = item.ProductBomId ?? string.Empty,
                ProductName = item.ProductName,
                ProductQty = item.ProductQty,
                ProductSeq = item.ProductSeq,
                UpdatedUser = item.UpdatedUser
            }).ToList();

            // 3. Adiciona em lote todos os itens novos
            if (newBoms.Any())
            {
                await _context.ProductPackingBom.AddRangeAsync(newBoms);
            }
        }

        public async Task DeleteBomsByProductIdAsync(string productId)
        {
            var boms = await _context.ProductPackingBom
                .Where(b => b.ProductId == productId)
                .ToListAsync();

            if (boms.Any())
            {
                _context.ProductPackingBom.RemoveRange(boms);
            }
        }

    }
}