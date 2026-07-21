using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Models.Enums;
using OxfordOnline.Repositories.Interfaces;
using OxfordOnline.Services;
using OxfordOnline.Utils;
using SkiaSharp;
using System.IO.Compression;

namespace OxfordOnline.Repositories
{
    public class ProductPackRepository : IProductPackRepository
    {
        private readonly AppDbContext _context;
        private readonly IImageRepository _imageRepository;
        private readonly IFtpService _ftpService;
        private readonly ILogger<ProductRepository> _logger;

        // --------------- Layout da imagem de BOM ---------------
        private const int ImageWidth = 1080;
        private const int MarginLeft = 140;
        private const int MarginRight = 110;
        private const int MarginY = 48;
        private const int HeaderHeight = 0;
        private const int TableHeaderHeight = 48;
        private const int RowPaddingY = 6;
        private const int QtyColumnWidth = 140;
        private const float LineHeight = 24f;

        private static readonly SKTypeface RegularTypeface = LoadTypeface("DejaVuSans.ttf");
        private static readonly SKTypeface BoldTypeface = LoadTypeface("DejaVuSans-Bold.ttf");


        public ProductPackRepository(AppDbContext context, IImageRepository imageRepository, IFtpService ftpService, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
            _imageRepository = imageRepository;
            _ftpService = ftpService;
        }

        /*
        private static SKTypeface LoadTypeface(string fileName)
        {
            var fontPath = Path.Combine(AppContext.BaseDirectory, "Fonts", fileName);

            if (File.Exists(fontPath))
            {
                var typeface = SKTypeface.FromFile(fontPath);
                if (typeface != null)
                    return typeface;
            }

            // Fallback: fonte default do SO. Só deve acontecer se o arquivo
            // não foi publicado corretamente — logar isso é responsabilidade
            // do chamador na primeira renderização com fallback.
            return SKTypeface.Default;
        }
        */

        /// <summary>
        /// Busca do EmbeddedResource e carregar via stream (mais robusto, não depende do disco)
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static SKTypeface LoadTypeface(string fileName)
        {
            var assembly = typeof(ProductPackRepository).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

            if (resourceName != null)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    var typeface = SKTypeface.FromStream(stream);
                    if (typeface != null)
                        return typeface;
                }
            }

            return SKTypeface.Default;
        }

        public async Task<string> GenerateAndUploadBomImageAsync(
            string productId,
            IEnumerable<ProductPackingBom> bomItems)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("productId é obrigatório.", nameof(productId));

            ArgumentNullException.ThrowIfNull(bomItems);

            var items = bomItems.OrderBy(b => b.ProductSeq).ToList();

            if (items.Count == 0)
            {
                _logger.LogWarning("Tentativa de gerar imagem de BOM sem itens para o produto '{ProductId}'.", productId);
                throw new InvalidOperationException($"Não há itens de BOM para o produto '{productId}'.");
            }

            byte[] imageBytes;
            try
            {
                // Renderização é CPU-bound e síncrona — roda fora das threads do pool de requisições.
                imageBytes = await Task.Run(() => RenderBomImage(productId, items));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao renderizar imagem de BOM para o produto '{ProductId}'.", productId);
                throw;
            }

            string remoteRelativePath;
            try
            {
                // Monta o caminho seguindo o mesmo padrão hierárquico usado para as demais imagens do produto,
                // buscando os dados do Oxford (marca, linha, decoração, família) para compor o diretório.
                var oxford = await _context.Oxford.FirstOrDefaultAsync(o => o.ProductId == productId);
                if (oxford == null)
                {
                    _logger.LogError("Produto não encontrado para gerar caminho de imagem de BOM: {ProductId}", productId);
                    throw new KeyNotFoundException($"Produto não encontrado: {productId}");
                }

                var pathBuilder = new FtpImagePathBuilder(
                    oxford.FamilyDescription.Replace(" ", "_"),
                    oxford.BrandDescription.Replace(" ", "_"),
                    oxford.LineDescription.Replace(" ", "_"),
                    oxford.DecorationDescription.Replace(" ", "_"),
                    oxford.ProductId,
                    "EMBALAGEM"
                );

                // Garante que o diretório de destino existe antes do upload.
                await _ftpService.EnsureDirectoryExistsAsync(pathBuilder);

                var directoryPath = pathBuilder.ToString();
                if (string.IsNullOrEmpty(directoryPath))
                    throw new InvalidOperationException("Não foi possível determinar o diretório de destino da imagem de BOM.");

                remoteRelativePath = $"{directoryPath}/BOM_{productId}.png";

                using var uploadStream = new MemoryStream(imageBytes);
                await _ftpService.UploadAsync(remoteRelativePath, uploadStream);

                // Registra/atualiza no banco o caminho da imagem de BOM gerada, na finalidade EMBALAGEM.
                await _imageRepository.SaveBomImagePathAsync(productId, remoteRelativePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar imagem de BOM ao FTP para o produto '{ProductId}'.", productId);
                throw;
            }

            _logger.LogInformation("Imagem de BOM gerada e enviada com sucesso para '{Path}'.", remoteRelativePath);
            return remoteRelativePath;
        }

        /// <summary>
        /// Renderiza a imagem em memória usando SkiaSharp. Método síncrono e puro — chamado via Task.Run.
        /// </summary>
        private static byte[] RenderBomImage(string productId, List<ProductPackingBom> items)
        {
            using var columnHeaderFont = new SKFont(BoldTypeface, 18);
            using var rowFont = new SKFont(RegularTypeface, 17);

            using var blackFill =       new SKPaint { Color = SKColors.Black,               IsAntialias = true };
            using var grayFill =        new SKPaint { Color = new SKColor(120, 120, 120),   IsAntialias = true };
            using var stripeFill =      new SKPaint { Color = new SKColor(245, 245, 245),   IsAntialias = true };
            using var linePaintDark =   new SKPaint { Color = SKColors.Black,               StrokeWidth = 2f, IsAntialias = true };
            using var linePaintGray =   new SKPaint { Color = new SKColor(180, 180, 180),   StrokeWidth = 1f, IsAntialias = true };

            var descriptionColumnWidth = ImageWidth - MarginLeft - MarginRight - QtyColumnWidth;

            // 1ª passada: trunca a descrição para caber em uma única linha (sem quebra)
            var rows = new List<(string Description, string Qty)>(items.Count);

            foreach (var item in items)
            {
                var description = string.IsNullOrWhiteSpace(item.ProductName)
                    ? "(Sem descrição)"
                    : item.ProductName!.Trim();

                var truncated = TruncateToFit(description, rowFont, descriptionColumnWidth - 4);
                rows.Add((truncated, item.ProductQty.ToString()));
            }

            var rowHeight = LineHeight + (RowPaddingY * 2);
            var tableHeight = rowHeight * rows.Count;

            var totalHeight = (int)Math.Ceiling(MarginY + HeaderHeight + TableHeaderHeight + tableHeight + MarginY);

            using var bitmap = new SKBitmap(ImageWidth, totalHeight);
            using var canvas = new SKCanvas(bitmap);

            canvas.Clear(SKColors.White);

            var currentY = MarginY + HeaderHeight;

            canvas.DrawText("DESCRIÇÃO", MarginLeft, currentY + 18, SKTextAlign.Left, columnHeaderFont, blackFill);
            canvas.DrawText("QTD", ImageWidth - MarginRight - (QtyColumnWidth / 2f), currentY + 18,
                SKTextAlign.Center, columnHeaderFont, blackFill);

            currentY += TableHeaderHeight;
            canvas.DrawLine(MarginLeft, currentY - 5, ImageWidth - MarginRight, currentY - 5, linePaintGray);

            for (var i = 0; i < rows.Count; i++)
            {
                var (description, qty) = rows[i];

                if (i % 2 == 1)
                {
                    canvas.DrawRect(MarginLeft, currentY, ImageWidth - MarginLeft - MarginRight, rowHeight, stripeFill);
                }

                var textY = currentY + RowPaddingY + LineHeight * 0.75f;
                canvas.DrawText(description, MarginLeft + 4, textY, SKTextAlign.Left, rowFont, blackFill);

                canvas.DrawText(qty, ImageWidth - MarginRight - (QtyColumnWidth / 2f),
                    currentY + RowPaddingY + LineHeight * 0.75f, SKTextAlign.Center, rowFont, blackFill);

                currentY += (int)rowHeight;
            }

            canvas.Flush();

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        /// <summary>
        /// Trunca o texto (sem quebrar linha) para caber na largura máxima informada, cortando o excesso.
        /// </summary>
        private static string TruncateToFit(string text, SKFont font, float maxWidth)
        {
            if (font.MeasureText(text) <= maxWidth)
                return text;

            var result = text;
            while (result.Length > 0 && font.MeasureText(result) > maxWidth)
            {
                result = result[..^1]; // remove o último caractere
            }

            return result;
        }

        /// <summary>
        /// Quebra o texto em múltiplas linhas para caber na largura máxima informada.
        /// </summary>
        private static string[] WrapText(string text, SKFont font, float maxWidth)
        {
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var lines = new List<string>();
            var currentLine = string.Empty;

            foreach (var word in words)
            {
                var candidate = currentLine.Length == 0 ? word : $"{currentLine} {word}";
                var width = font.MeasureText(candidate);

                if (width > maxWidth && currentLine.Length > 0)
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine = candidate;
                }
            }

            if (currentLine.Length > 0)
            {
                lines.Add(currentLine);
            }

            return lines.Count == 0 ? new[] { string.Empty } : lines.ToArray();
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

        /*
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
        */

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

            // 4. Gera e envia a imagem da sequência de embalagem (SkiaSharp) refletindo o novo BOM.
            //    Isso é feito "best effort": se falhar, não deve impedir o upsert do BOM em si,
            //    já que a imagem é um artefato derivado (cache visual), não a fonte da verdade.
            if (newBoms.Any())
            {
                try
                {
                    string remotePath = await GenerateAndUploadBomImageAsync(request.ProductId, newBoms);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Falha ao gerar/enviar imagem de BOM durante o upsert do produto '{ProductId}'. " +
                        "Os itens de BOM foram atualizados normalmente, apenas a imagem não foi atualizada.",
                        request.ProductId);
                }
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