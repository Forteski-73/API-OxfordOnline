using OxfordOnline.Models;
using OxfordOnline.Models.Dto;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface IProductPackRepository
    {
        Task<IEnumerable<PalletGroup>> GetAllAsync();
        Task<PalletGroup?> GetByIdAsync(int id);
        Task<IEnumerable<PalletGroup>> GetPacksByProductIdAsync(string productId);
        Task AddAsync(PalletGroup pack);
        Task UpdateAsync(PalletGroup pack);
        Task DeleteAsync(PalletGroup pack);
        Task SaveAsync();

        // Busca todas as imagens de um pacote específico
        Task<IEnumerable<PalletGroupImage>> GetImagesByPackIdAsync(int packId);

        // Busca todas as imagens de um pacote específico
        Task<IEnumerable<ImagePackBase64>> GetPackImagesAsBase64Async(int packId);

        // Busca uma imagem específica (Chave composta: ID do Pacote + Sequência)
        Task<PalletGroupImage?> GetImageAsync(int packId, int sequence);

        // Adiciona uma nova imagem à tabela product_pack_image
        Task AddImageAsync(PalletGroupImage image);

        // Remove uma imagem da tabela product_pack_image
        Task DeleteImageAsync(PalletGroupImage image);

        // remove a imagem do banco
        Task DeleteByPackIdAsync(int packId);

        /// <summary>
        /// Busca todos os itens associados a um packId específico
        /// </summary>
        Task<IEnumerable<PalletGroupItem>> GetItemsByPackIdAsync(int packId);

        /// <summary>
        /// Busca um item específico através da chave composta (ID do Pacote + SKU/PackItem)
        /// </summary>
        Task<PalletGroupItem?> GetItemAsync(int packId, string sku);

        /// <summary>
        /// Adiciona um novo item à tabela product_pack_item
        /// </summary>
        Task<PalletGroupItem> AddItemAsync(PalletGroupItem item);

        /// <summary>
        /// Remove um registro da tabela product_pack_item
        /// </summary>
        Task DeleteItemAsync(PalletGroupItem item);

        /// <summary>
        /// Remove um registro da tabela product_pack
        /// </summary>
        Task DeleteItemsByPackIdAsync(int packId);


        // --------------- Métodos para BOM (product_packing_bom) ---------------

        /// <summary>
        /// Busca todas as estruturas de packing BOM de um produto específico ordenadas por sequência
        /// </summary>
        Task<IEnumerable<ProductPackingBom>> GetBomsByProductIdAsync(string productId);

        /// <summary>
        /// Insere um novo registro ou atualiza um existente utilizando a chave única composta (ProductId + ProductBomId + ProductSeq)
        /// </summary>
        Task UpsertBomAsync(ProductPackingBomRequest request);


        /// <summary>
        /// Remove em lote todos os registros de packing BOM associados a um produto principal
        /// </summary>
        Task DeleteBomsByProductIdAsync(string productId);
    }
}