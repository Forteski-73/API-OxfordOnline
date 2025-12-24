// Localização: OxfordOnline.Services.Interfaces/IInventoryRepository.cs (ou onde você a utiliza)

using OxfordOnline.Models;
using OxfordOnline.Models.Dto;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface IInventoryRepository
    {
        // --- Métodos de Controle de Persistência (Anteriormente no Data Repository) ---
        Task SaveAsync();

        // -----------------------------------------------------------------------------
        // --- InventoryGuid ---
        // -----------------------------------------------------------------------------

        // Métodos de Acesso a Dados (CRUD Básico)
        Task<InventoryGuid?> GetGuidByInventGuidAsync(string inventGuid);
        Task AddGuidAsync(InventoryGuid inventoryGuid);
        Task<bool> GuidExistsAsync(string inventGuid);
        Task<IEnumerable<InventoryGuid>> GetAllInventoryGuidsAsync();

        // Métodos de Lógica de Negócio (Service)
        // Retorna false se já existe (comportamento idempotente)
        Task<bool> CreateInventoryGuidAsync(InventoryGuid inventoryGuid);


        // -----------------------------------------------------------------------------
        // --- Inventory ---
        // -----------------------------------------------------------------------------

        // Métodos de Acesso a Dados (CRUD Básico)
        Task<Inventory?> GetInventoryByGuidAsync(string guid);

        Task<Inventory?> GetInventoryByGuidInventCodeAsync(string guid, string inventCode);

        Task<Inventory?> GetInventoryByCodeAsync(string inventCode);
        Task AddInventoryAsync(Inventory inventory);
        void UpdateInventory(Inventory inventory);
        void DeleteInventory(Inventory inventory);

        // Métodos de Lógica de Negócio (Service)
        // Lógica de Update ou Insert
        Task<bool> CreateOrUpdateInventoryAsync(Inventory inventory);

        Task<List<Inventory>> GetRecentInventoriesByGuid(string _inventCode);

        Task<bool> DeleteInventoryAsync(string _inventCode); // Usando a lógica de Service (que pode incluir exclusão de Records)

        /// Retorna todas as máscaras configuradas para os campos (Unitizador, Posição, Código)
        Task<IEnumerable<InventoryMask>> GetAllInventoryMasksAsync();

        /// <summary>
        /// Retorna o total de registros na tabela de produtos.
        /// </summary>
        Task<int> GetProductCountAsync();

        /// <summary>
        /// Retorna uma lista paginada de produtos.
        /// Retornamos 'object' ou um DTO específico se você não quiser expor a model inteira.
        /// </summary>
        Task<IEnumerable<object>> GetProductsPagedAsync(int pageNumber, int pageSize = 10000);

        // -----------------------------------------------------------------------------
        // --- InventoryRecord ---
        // -----------------------------------------------------------------------------

        // Métodos de Acesso a Dados (CRUD Básico/Lote)
        Task<List<InventoryRecord>> GetRecordsByInventCodeAsync(string inventCode);
        Task<InventoryRecord?> GetRecordByIdAsync(string inventCode);
        Task<InventoryRecord?> GetRecordByUniqueKeysAsync(string inventCode, string inventLocation, string inventBarcode);

        void AddRangeRecords(List<InventoryRecord> records);
        void UpdateRangeRecords(List<InventoryRecord> records);
        void DeleteRecord(InventoryRecord record);

        // Lógica de Agregação de Dados
        Task<decimal> CalculateInventoryTotalAsync(string inventCode);

        // Métodos de Lógica de Negócio (Service)
        // Lógica de BATCH Update/Insert e recalculo do total do Inventory pai
        Task<(int created, int updated)> CreateOrUpdateInventoryRecordsAsync(List<InventoryRecordRequest> records);
        Task<bool> DeleteInventoryRecordAsync(string inventCode); // Usando a lógica de Service (que inclui recalculo do total)
    }
}