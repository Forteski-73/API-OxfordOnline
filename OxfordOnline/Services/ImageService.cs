using OxfordOnline.Models;
using OxfordOnline.Models.Enums;
using OxfordOnline.Repositories;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Services
{
    public class ImageService
    {
        private readonly IImageRepository   _imageRepository;
        private readonly IProductRepository _productRepository;
        private readonly IFtpService        _ftpService;

        public ImageService(IImageRepository imageRepository, IProductRepository productRepository)
        {
            _imageRepository = imageRepository;
            _productRepository = productRepository;
        }

        public async Task<Image?> GetImageByIdAsync(int id)
        {
            return await _imageRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Image>> GetImagesByProductIdAsync(string productId, Finalidade finalidade, bool main)
        {
            return await _imageRepository.GetByProductIdAsync(productId, finalidade, main);
        }

        public async Task<IEnumerable<Image>> CreateOrReplaceImagesAsync(List<Image> images)
        {
            var productIds = images.Select(i => i.ProductId).Distinct().ToList();

            // Consultar o repositório correto para produtos
            //var existingProducts = await _imageRepository.GetByProductIdsAsync(productIds);
            var existingProducts = await _productRepository.GetByProductListIdsAsync(productIds);
            var existingProductIds = existingProducts.Select(p => p.ProductId).ToList();

            var missingProductIds = productIds.Except(existingProductIds).ToList();

            if (missingProductIds.Any())
                throw new ArgumentException($"Produtos não encontrados: {string.Join(", ", missingProductIds)}");

            // Remover imagens antigas dos produtos recebidos
            await _imageRepository.RemoveByProductIdsAsync(productIds);

            // Evitar rastreamento duplicado no EF
            foreach (var img in images)
                img.Product = null;

            await _imageRepository.AddRangeAsync(images);
            await _imageRepository.SaveAsync();

            return images;
        }

        /*
        public async Task DeleteImagesByProductIdAsync(string productId)
        {
            await _imageRepository.DeleteImagesByProductIdAsync(productId);
        }

        // NOVO: Salva uma nova imagem, envia para o FTP e grava no banco
        public async Task SaveImageAsync(string productId, string fileName, Stream content)
        {
            await _imageRepository.SaveImageAsync(productId, fileName, content);
        }
        */

        public async Task UpdateImagesByProductIdAsync(string productId, Finalidade finalidade, List<IFormFile> files)
        {
            await _imageRepository.UpdateImagesByProductIdAsync(productId, finalidade, files);
        }

        // Na sua classe de serviço
        public async Task UpdateImagesByteAsync(string productId, Finalidade finalidade, List<byte[]> imageBytesList)
        {
            // ... sua lógica de upload aqui, usando os bytes diretamente
            await _imageRepository.UpdateImagesByteAsync(productId, finalidade, imageBytesList);
        }

        public async Task DeleteAllImagesByProductIdAsync(string productId)
        {
            await _imageRepository.DeleteAllImagesByProductIdAsync(productId);
        }

        // NOVO: Baixa imagem direto do FTP como stream
        public async Task<Stream> DownloadImageStreamAsync(string ftpFilePath)
        {
            return await _imageRepository.DownloadFileStreamFromFtpAsync(ftpFilePath);
        }
    }
}
