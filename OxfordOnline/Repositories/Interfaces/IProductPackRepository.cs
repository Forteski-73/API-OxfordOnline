using OxfordOnline.Models;
using OxfordOnline.Models.Dto;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface IProductPackRepository
    {
        Task<IEnumerable<ProductPack>> GetAllAsync();
        Task<ProductPack?> GetByIdAsync(int id);
        Task<IEnumerable<ProductPack>> GetPacksByProductIdAsync(string productId);
        Task AddAsync(ProductPack pack);
        Task UpdateAsync(ProductPack pack);
        Task DeleteAsync(ProductPack pack);
        Task SaveAsync();

        // --- Novos métodos para Imagens ---

        // Busca todas as imagens de um pacote específico
        Task<IEnumerable<ProductPackImage>> GetImagesByPackIdAsync(int packId);

        // Busca todas as imagens de um pacote específico
        Task<IEnumerable<ImagePackBase64>> GetPackImagesAsBase64Async(int packId);

        // Busca uma imagem específica (Chave composta: ID do Pacote + Sequência)
        Task<ProductPackImage?> GetImageAsync(int packId, int sequence);

        // Adiciona uma nova imagem à tabela product_pack_image
        Task AddImageAsync(ProductPackImage image);

        // Remove uma imagem da tabela product_pack_image
        Task DeleteImageAsync(ProductPackImage image);

        // remove a imagem do banco
        Task DeleteByPackIdAsync(int packId);
    }
}