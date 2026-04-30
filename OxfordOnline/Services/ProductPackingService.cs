using Microsoft.EntityFrameworkCore;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Models.Enums;
using OxfordOnline.Repositories;
using OxfordOnline.Repositories.Interfaces;
using System.IO.Compression;
using static System.Net.Mime.MediaTypeNames;

namespace OxfordOnline.Services
{
    public class ProductPackingService
    {
        private readonly IProductPackRepository _repo;

        public ProductPackingService(IProductPackRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ProductPack>> GetAllPacksAsync()
        {
            // O repositório deve implementar a lógica de Include para Images e Items
            return await _repo.GetAllAsync();
        }

        public async Task<ProductPack?> GetPackByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<ProductPack> CreatePackAsync(ProductPack pack)
        {
            // Garante a data de criação no servidor
            pack.PackCreated = DateTime.Now;

            await _repo.AddAsync(pack);
            await _repo.SaveAsync();

            return pack;
        }

        public async Task<bool> UpdatePackAsync(ProductPack pack)
        {
            var existingPack = await _repo.GetByIdAsync(pack.PackId);
            if (existingPack == null) return false;

            // Atualiza propriedades básicas
            existingPack.PackName = pack.PackName;
            existingPack.PackUser = pack.PackUser;

            // Aqui a lógica de atualização de Itens e Imagens depende de como 
            // seu repositório lida com o rastreamento do EF Core.
            await _repo.UpdateAsync(existingPack);
            await _repo.SaveAsync();

            return true;
        }

        public async Task<bool> DeletePackAsync(int id)
        {
            var pack = await _repo.GetByIdAsync(id);
            if (pack == null) return false;

            await _repo.DeleteAsync(pack);
            await _repo.SaveAsync();

            return true;
        }

        public async Task<IEnumerable<ProductPack>> GetPacksByProductAsync(string productId)
        {
            // Busca pacotes que contenham o item específico
            return await _repo.GetPacksByProductIdAsync(productId);
        }

        public async Task<IEnumerable<ImagePackBase64>> GetImagesByPackAsync(int packId)
        {
            // O repositório deve buscar na tabela product_pack_image
            return await _repo.GetPackImagesAsBase64Async(packId);
        }

        public async Task<ProductPackImage> AddImageAsync(ProductPackImage image)
        {
            image.PackLastUpdate = DateTime.Now;
            await _repo.AddImageAsync(image);
            await _repo.SaveAsync();
            return image;
        }

        public async Task<bool> DeleteImageAsync(int packId, int sequence)
        {
            var image = await _repo.GetImageAsync(packId, sequence);
            if (image == null) return false;

            await _repo.DeleteImageAsync(image);
            await _repo.SaveAsync();
            return true;
        }

        public async Task DeleteAllImagesByPackIdAsync(int packId)
        {
            await _repo.DeleteByPackIdAsync(packId);
        }

        // --- Métodos de Itens (Tabela Filha product_pack_item) ---

        /// <summary>
        /// Busca todos os itens (SKUs) associados a um pacote específico.
        /// </summary>
        public async Task<IEnumerable<ProductPackItem>> GetItemsByPackAsync(int packId)
        {
            // O repositório deve buscar na tabela product_pack_item filtrando pelo packId
            return await _repo.GetItemsByPackIdAsync(packId);
        }

        /// <summary>
        /// Adiciona um novo item (produto) ao esquema de montagem.
        /// </summary>
        public async Task<ProductPackItem> AddItemAsync(ProductPackItem item)
        {
            // Opcional: Você pode adicionar validações de regra de negócio aqui
            // antes de enviar para o repositório.

            await _repo.AddItemAsync(item);
            await _repo.SaveAsync();
            return item;
        }

        /// <summary>
        /// Remove um item específico de um pacote baseado no PackId e no SKU (PackItem).
        /// </summary>
        public async Task<bool> DeleteItemAsync(int packId, string sku)
        {
            // Busca o item específico no repositório
            var item = await _repo.GetItemAsync(packId, sku);

            if (item == null) return false;

            await _repo.DeleteItemAsync(item);
            await _repo.SaveAsync();

            return true;
        }

    }
}