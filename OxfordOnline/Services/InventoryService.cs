// Localização: OxfordOnline.Services/InventoryService.cs

using Microsoft.EntityFrameworkCore;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Models.Dtos;
using OxfordOnline.Repositories.Interfaces;
using OxfordOnline.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxfordOnline.Services
{
    /// <summary>
    /// Camada de Serviço de Inventário.
    /// Delega todas as operações, tanto de CRUD quanto de lógica,
    /// para o IInventoryRepository (que atua como Service e Data Layer unificados).
    /// </summary>
    public class InventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        // Injeção de dependência do repositório/serviço unificado
        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        // -----------------------------------------------------------------------------
        // --- InventoryGuid - Métodos do Serviço (Delegados ao Repositório) ---
        // -----------------------------------------------------------------------------

        public async Task<InventoryGuid?> GetGuidByInventGuidAsync(string inventGuid) =>
            await _inventoryRepository.GetGuidByInventGuidAsync(inventGuid);

        public async Task<(bool created, bool updated)> CreateOrUpdateInventoryGuidAsync(InventoryGuid inventoryGuid) =>
            await _inventoryRepository.CreateOrUpdateInventoryGuidAsync(inventoryGuid);

        public async Task<IEnumerable<InventoryGuid>> GetAllInventoryGuidsAsync() =>
            await _inventoryRepository.GetAllInventoryGuidsAsync();

        // -----------------------------------------------------------------------------
        // --- Inventory - Métodos do Serviço (Delegados ao Repositório) ---
        // -----------------------------------------------------------------------------

        public async Task<Inventory?> GetInventoryByGuidAsync(string guid) =>
            await _inventoryRepository.GetInventoryByGuidAsync(guid);

        public async Task<Inventory?> GetInventoryByGuidInventCodeAsync(string _guid, string _inventCode) =>
            await _inventoryRepository.GetInventoryByGuidInventCodeAsync(_guid, _inventCode);

        public async Task<bool> CreateOrUpdateInventoryAsync(Inventory inventory) =>
            await _inventoryRepository.CreateOrUpdateInventoryAsync(inventory);

        public async Task<List<Inventory>> GetRecentInventoriesByGuid(string inventGuid) =>
            await _inventoryRepository.GetRecentInventoriesByGuid(inventGuid);

        public async Task<List<Inventory>> GetInventoryAllAsync() =>
            await _inventoryRepository.GetInventoryAllAsync();

        public async Task<bool> DeleteInventoryAsync(string _inventCode) =>
            await _inventoryRepository.DeleteInventoryAsync(_inventCode);

        // --- Exemplo adicional: Método que combina o Get + Update ---
        public async Task<bool> UpdateInventoryStatusAsync(string inventCode, string newStatus)
        {
            var existingInventory = await _inventoryRepository.GetInventoryByCodeAsync(inventCode);
            if (existingInventory == null) return false;

            // Lógica de Serviço: Altera apenas o status
            existingInventory.InventStatus = newStatus;

            // Marca para atualização (método de baixo nível)
            _inventoryRepository.UpdateInventory(existingInventory);

            // Persiste
            await _inventoryRepository.SaveAsync();
            return true;
        }

        public async Task<int> GetProductCountAsync() =>
            await _inventoryRepository.GetProductCountAsync();


        public async Task<IEnumerable<object>> GetProductsPagedAsync(int pageNumber, int pageSize = 10000) =>
            await _inventoryRepository.GetProductsPagedAsync(pageNumber, pageSize);


        //public async Task<InventoryGuid?> GetInventAllAsync(string inventGuid) =>
         //   await _inventoryRepository.GetInventAllAsync(inventGuid);

        // -----------------------------------------------------------------------------
        // --- InventoryRecord - Métodos do Serviço (Delegados ao Repositório) ---
        // -----------------------------------------------------------------------------

        public async Task<List<InventoryRecord>> GetRecordsByInventCodeAsync(string inventCode) =>
            await _inventoryRepository.GetRecordsByInventCodeAsync(inventCode);

        public async Task<InventoryRecord?> GetRecordByIdAsync(int inventId) =>
            await _inventoryRepository.GetRecordByIdAsync(inventId);

        public async Task<(int created, int updated)> CreateOrUpdateInventoryRecordsAsync(List<InventoryRecordRequest> records) =>
            await _inventoryRepository.CreateOrUpdateInventoryRecordsAsync(records);

        public async Task<bool> DeleteInventoryRecordAsync(int inventId) =>
            await _inventoryRepository.DeleteInventoryRecordAsync(inventId);
        
        public async Task<bool> DeleteInvRecByCodeItemAsync(string inventCode, string unitizer, string location, string item) =>
            await _inventoryRepository.DeleteInvRecByCodeItemAsync(inventCode, unitizer, location, item);

        // -----------------------------------------------------------------------------
        // --- InventSum - Métodos do Serviço (Delegados ao Repositório) ---
        // -----------------------------------------------------------------------------
        public async Task<IEnumerable<InventoryAuditResult>> GetInventoryAuditResultAsync(string inventLocationId) => 
            await _inventoryRepository.GetInventoryAuditResultAsync(inventLocationId);

        // -----------------------------------------------------------------------------
        // --- InventoryMask - Métodos do Serviço (Delegados ao Repositório) ---
        // -----------------------------------------------------------------------------
        public async Task<IEnumerable<InventoryMask>> GetAllInventoryMasksAsync() =>
            await _inventoryRepository.GetAllInventoryMasksAsync();

        // -----------------------------------------------------------------------------
        // --- InventoryHeader - Métodos do Serviço (Delegados ao Repositório) ---
        // -----------------------------------------------------------------------------
        public async Task<InventoryHeader?> GetInventoryHeaderByIdAsync(int id) =>
            await _inventoryRepository.GetInventoryHeaderByIdAsync(id);

        public async Task<bool> CreateOrUpdateInventoryHeaderAsync(InventoryHeader header) =>
            await _inventoryRepository.CreateOrUpdateInventoryHeaderAsync(header);

        public async Task<List<InventoryHeader>> GetRecentActiveInventoryHeadersAsync(int count = 12) =>
            await _inventoryRepository.GetRecentActiveInventoryHeadersAsync(count);

        // -----------------------------------------------------------------------------
        // --- Métodos de Utilidade/Consulta ---
        // -----------------------------------------------------------------------------

        public async Task<decimal> GetTotalInventoryQuantityAsync(string inventCode) =>
            await _inventoryRepository.CalculateInventoryTotalAsync(inventCode);

    }
}