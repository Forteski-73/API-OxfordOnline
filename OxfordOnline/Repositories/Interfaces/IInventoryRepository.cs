// Localização: OxfordOnline.Services.Interfaces/IInventoryRepository.cs (ou onde você a utiliza)

using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Models.Dtos;

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
        // Insere se não existir; se já existir, atualiza o invent_header_id quando ele for diferente
        Task<(bool created, bool updated)> CreateOrUpdateInventoryGuidAsync(InventoryGuid inventoryGuid);


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

        Task<List<Inventory>> GetInventoryAllAsync();

        Task<bool> DeleteInventoryAsync(string _inventCode); // Usando a lógica de Service (que pode incluir exclusão de Records)

        // -----------------------------------------------------------------------------------
        // --- Retorna todos os registros de saldo/disponibilidade de estoque (invent_sum) ---
        // -----------------------------------------------------------------------------------
        Task<IEnumerable<InventoryAuditResult>> GetInventoryAuditResultAsync(string inventLocationId);

        /// Retorna todas as máscaras configuradas para os campos (Unitizador, Posição, Código)
        Task<IEnumerable<InventoryMask>> GetAllInventoryMasksAsync();

        // -----------------------------------------------------------------------------
        // --- InventoryHeader ---
        // -----------------------------------------------------------------------------

        Task<InventoryHeader?> GetInventoryHeaderByIdAsync(int id);

        // Lógica: Update (se Id > 0) ou Insert
        Task<bool> CreateOrUpdateInventoryHeaderAsync(InventoryHeader header);

        // Retorna os últimos N headers ativos cadastrados (ordenados por Id decrescente)
        Task<List<InventoryHeader>> GetRecentActiveInventoryHeadersAsync(int count = 12);

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
        Task<InventoryRecord?> GetRecordByIdAsync(int inventId);
        Task<InventoryRecord?> GetRecordByUniqueKeysAsync(string inventCode, string inventLocation, string inventBarcode);

        void AddRangeRecords(List<InventoryRecord> records);
        void UpdateRangeRecords(List<InventoryRecord> records);
        void DeleteRecord(InventoryRecord record);

        // Lógica de Agregação de Dados
        Task<decimal> CalculateInventoryTotalAsync(string inventCode);

        // Métodos de Lógica de Negócio (Service)
        // Lógica de BATCH Update/Insert e recalculo do total do Inventory pai
        Task<(int created, int updated)> CreateOrUpdateInventoryRecordsAsync(List<InventoryRecordRequest> records);
        Task<bool> DeleteInventoryRecordAsync(int inventId); // Usando a lógica de Service (que inclui recalculo do total)
        Task<bool> DeleteInvRecByCodeItemAsync(string inventCode, string unitizer, string location, string item); // Usando a lógica de Service (que inclui recalculo do total)
    }
}