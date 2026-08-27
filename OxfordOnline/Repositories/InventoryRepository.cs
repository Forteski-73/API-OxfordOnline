// Localização: OxfordOnline.Repositories/InventoryRepository.cs

using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Models.Dtos;
using OxfordOnline.Repositories.Interfaces; // Usa a interface unificada
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxfordOnline.Repositories
{
    // Esta classe implementa a interface ÚNICA que contém tanto a lógica de negócio quanto o acesso a dados.
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        // -----------------------------------------------------------------------------
        // --- Implementação dos Métodos de Persistência e Acesso a Dados (CRUD Básico) ---
        // -----------------------------------------------------------------------------

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        // --- InventoryGuid - CRUD Básico ---

        public Task<InventoryGuid?> GetGuidByInventGuidAsync(string inventGuid)
        {
            return _context.InventoryGuid
                .FirstOrDefaultAsync(g => g.InventGuid == inventGuid);
        }

        public async Task AddGuidAsync(InventoryGuid inventoryGuid)
        {
            inventoryGuid.InventCreated ??= DateTime.Now;
            await _context.InventoryGuid.AddAsync(inventoryGuid);
        }

        public Task<bool> GuidExistsAsync(string inventGuid)
        {
            return _context.InventoryGuid.AnyAsync(g => g.InventGuid == inventGuid);
        }

        public Task<bool> InventoryExistsAsync(string _inventCode)
        {
            return _context.Inventory.AnyAsync(g => g.InventCode == _inventCode);
        }

        public Task<IEnumerable<InventoryGuid>> GetAllInventoryGuidsAsync()
        {
            return Task.FromResult<IEnumerable<InventoryGuid>>(_context.InventoryGuid.AsNoTracking().ToList());
        }

        // --- Inventory - CRUD Básico ---

        public Task<Inventory?> GetInventoryByGuidAsync(string _inventCode)
        {
            return _context.Inventory.FirstOrDefaultAsync(i => i.InventCode == _inventCode);
        }

        public Task<Inventory?> GetInventoryByCodeAsync(string inventCode)
        {
            return _context.Inventory.FirstOrDefaultAsync(i => i.InventCode == inventCode);
        }

        public Task<Inventory?> GetInventoryByGuidInventCodeAsync(string inventGuid, string inventCode)
        {
            return _context.Inventory.FirstOrDefaultAsync(i =>
                i.InventGuid.ToLower() == inventGuid.ToLower() &&
                i.InventCode.ToLower() == inventCode.ToLower()
            );
        }

        public async Task AddInventoryAsync(Inventory inventory)
        {
            inventory.InventCreated ??= DateTime.Now;
            await _context.Inventory.AddAsync(inventory);
        }

        public void UpdateInventory(Inventory inventory)
        {
            _context.Inventory.Update(inventory);
        }

        public void DeleteInventory(Inventory inventory)
        {
            _context.Inventory.Remove(inventory);
        }

        // --- InventoryRecord - CRUD Básico/Lote ---

        /*public Task<List<InventoryRecord>> GetRecordsByInventCodeAsync(string inventCode)
        {
            return _context.InventoryRecord
                .Where(r => r.InventCode == inventCode)
                .ToListAsync();
        }*/

        public async Task<List<InventoryRecord>> GetRecordsByInventCodeAsync(string inventCode)
        {
            return await _context.InventoryRecord
                .Include(r => r.ProductNavigation) // Carrega a tabela de produto
                .Where(r => r.InventCode == inventCode)
                .Select(r => new InventoryRecord
                {
                    // Copia todas as propriedades necessárias
                    Id = r.Id,
                    InventCode = r.InventCode,
                    InventCreated = r.InventCreated,
                    InventUser = r.InventUser,
                    InventUnitizer = r.InventUnitizer,
                    InventLocation = r.InventLocation,
                    InventProduct = r.InventProduct,
                    InventBarcode = r.InventBarcode,
                    InventStandardStack = r.InventStandardStack,
                    InventQtdStack = r.InventQtdStack,
                    InventQtdIndividual = r.InventQtdIndividual,
                    InventTotal = r.InventTotal,

                    // Atribui a descrição do produto ao campo NotMapped
                    ProductDescription = r.ProductNavigation != null ? r.ProductNavigation.ProductName : "Sem Descrição"
                })
                .ToListAsync();
        }

        public Task<InventoryRecord?> GetRecordByIdAsync(int _inventId)
        {
            return _context.InventoryRecord.FindAsync(_inventId).AsTask();
        }

        public Task<InventoryRecord?> GetRecordByUniqueKeysAsync(string inventCode, string inventLocation, string inventBarcode)
        {
            return _context.InventoryRecord
                .FirstOrDefaultAsync(r =>
                    r.InventCode == inventCode &&
                    r.InventLocation == inventLocation &&
                    r.InventBarcode == inventBarcode);
        }

        public void AddRangeRecords(List<InventoryRecord> records)
        {
            foreach (var record in records)
            {
                record.InventCreated ??= DateTime.Now;
            }
            _context.InventoryRecord.AddRange(records);
        }

        public void UpdateRangeRecords(List<InventoryRecord> records)
        {
            _context.InventoryRecord.UpdateRange(records);
        }

        public void DeleteRecord(InventoryRecord record)
        {
            _context.InventoryRecord.Remove(record);
        }
        /*public async Task<IEnumerable<InventSum>> GetAllInventSumAsync()
        {
            return await _context.InventSum
                                 .AsNoTracking()
                                 .OrderBy(i => i.ProductId)
                                 .ThenBy(i => i.InventLocationId)
                                 .Take(100)
                                 .ToListAsync();
        }*/

        public async Task<IEnumerable<InventoryAuditResult>> GetInventoryAuditResultAsync(string inventLocationId)
        {
            var query =
                from ir in _context.InventoryRecord.AsNoTracking()
                join s in _context.InventSum.AsNoTracking()
                    on ir.InventProduct equals s.ProductId
                where s.InventLocationId == inventLocationId
                group new { ir, s } by new { s.InventLocationId, ir.InventProduct, ir.InventBarcode } into g
                orderby g.Key.InventProduct
                select new InventoryAuditResult
                {
                    InventLocationId = g.Key.InventLocationId,
                    InventProduct = g.Key.InventProduct,
                    InventBarcode = g.Key.InventBarcode,
                    TotalInvent = g.Sum(x => x.ir.InventTotal ?? 0),
                    AvailPhysical = g.Max(x => x.s.AvailPhysical)
                };

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<InventoryMask>> GetAllInventoryMasksAsync()
        {
            return await _context.InventoryMask
                                 .AsNoTracking()
                                 .OrderBy(m => m.Id)
                                 .ToListAsync();
        }

        // --- InventoryHeader - CRUD Básico ---

        public Task<InventoryHeader?> GetInventoryHeaderByIdAsync(int id)
        {
            return _context.InventoryHeader.FirstOrDefaultAsync(h => h.Id == id);
        }

        // --- InventoryHeader - Lógica de Service ---

        public async Task<bool> CreateOrUpdateInventoryHeaderAsync(InventoryHeader header)
        {
            if (string.IsNullOrWhiteSpace(header.InventName))
                throw new ArgumentException("O campo 'InventName' é obrigatório.");

            if (header.Id > 0)
            {
                var existing = await GetInventoryHeaderByIdAsync(header.Id);
                if (existing == null)
                    throw new KeyNotFoundException($"Inventário (header) com Id {header.Id} não encontrado.");

                var nameChanged = existing.InventName != header.InventName;

                existing.InventName = header.InventName;
                existing.InventDescription = header.InventDescription;
                existing.SalesChannelOnly = header.SalesChannelOnly;
                existing.Active = header.Active;

                // Propaga o novo nome para todos os Inventory filhos vinculados a este Header
                if (nameChanged)
                {
                    await _context.Inventory
                        .Where(i => i.InventHeaderId == header.Id)
                        .ExecuteUpdateAsync(s => s.SetProperty(i => i.InventName, header.InventName));
                }

                await SaveAsync();
                return false; // atualizado
            }

            await _context.InventoryHeader.AddAsync(header);
            await SaveAsync();
            return true; // criado
        }

        public async Task<List<InventoryHeader>> GetRecentActiveInventoryHeadersAsync(int count = 12)
        {
            return await _context.InventoryHeader
                .AsNoTracking()
                .Where(h => h.Active)
                .OrderByDescending(h => h.Id)
                .Take(count)
                .ToListAsync();
        }

        // --- Lógica de Agregação de Dados ---
        public async Task<decimal> CalculateInventoryTotalAsync(string inventCode)
        {
            // O SumAsync com um campo int? retorna um int?.
            decimal? totalIntNullable = await _context.InventoryRecord
                .Where(r => r.InventCode == inventCode)
                .SumAsync(r => r.InventTotal);

            // Retorna o valor (int) ou 0 se for nulo (se não houver registros).
            return totalIntNullable ?? 0;
        }

        // -----------------------------------------------------------------------------
        // --- Implementação dos Métodos de Lógica de Negócio (Service Layer) ---
        // -----------------------------------------------------------------------------

        // --- InventoryGuid - Lógica de Service ---

        public async Task<(bool created, bool updated)> CreateOrUpdateInventoryGuidAsync(InventoryGuid inventoryGuid)
        {
            if (string.IsNullOrWhiteSpace(inventoryGuid.InventGuid))
                throw new ArgumentException("O campo 'InventGuid' é obrigatório.");

            // Validação de Regra de Negócio: O Header deve existir
            var header = await GetInventoryHeaderByIdAsync(inventoryGuid.InventHeaderId);
            if (header == null)
                throw new KeyNotFoundException($"O Header de inventário '{inventoryGuid.InventHeaderId}' não foi encontrado em InventoryHeader.");

            var existing = await GetGuidByInventGuidAsync(inventoryGuid.InventGuid);
            if (existing == null)
            {
                await AddGuidAsync(inventoryGuid);
                await SaveAsync();
                return (true, false);
            }

            // Já existe: atualiza os campos editáveis que vierem diferentes do gravado
            var changed = false;

            if (existing.InventHeaderId != inventoryGuid.InventHeaderId)
            {
                existing.InventHeaderId = inventoryGuid.InventHeaderId;
                changed = true;
            }

            if (existing.InventExpSeq != inventoryGuid.InventExpSeq)
            {
                existing.InventExpSeq = inventoryGuid.InventExpSeq;
                changed = true;
            }

            if (!changed)
                return (false, false);

            await SaveAsync();
            return (false, true);
        }

        // --- Inventory - Lógica de Service ---

        public async Task<bool> CreateOrUpdateInventoryAsync(Inventory inventory)
        {
            if (string.IsNullOrWhiteSpace(inventory.InventGuid) || string.IsNullOrWhiteSpace(inventory.InventCode))
                throw new ArgumentException("Os campos 'InventGuid' e 'InventCode' são obrigatórios.");

            // 1. Validação de Regra de Negócio: O GUID deve existir
            var guidExists = await GuidExistsAsync(inventory.InventGuid);
            if (!guidExists)
                throw new KeyNotFoundException($"O GUID de inventário '{inventory.InventGuid}' não foi encontrado em InventoryGuid.");

            // 2. Validação de Regra de Negócio: O Header deve existir.
            // O 'invent_name' é sempre espelhado a partir do Header (fonte da verdade).
            var header = await GetInventoryHeaderByIdAsync(inventory.InventHeaderId);
            if (header == null)
                throw new KeyNotFoundException($"O Header de inventário '{inventory.InventHeaderId}' não foi encontrado em InventoryHeader.");

            inventory.InventName = header.InventName;

            var existingInventory = await GetInventoryByGuidAsync(inventory.InventCode);

            // 3. Lógica: Update ou Insert?
            if (existingInventory == null || string.IsNullOrWhiteSpace(existingInventory.InventCode))
            {
                // Tenta INSERIR
                await AddInventoryAsync(inventory);
            }
            else
            {
                // Mapeamento e marcação para atualização
                existingInventory.InventGuid = inventory.InventGuid;
                existingInventory.InventCode = inventory.InventCode;
                existingInventory.InventHeaderId = inventory.InventHeaderId;
                existingInventory.InventName = inventory.InventName;
                existingInventory.InventSector = inventory.InventSector;
                existingInventory.InventUser = inventory.InventUser;
                existingInventory.InventStatus = inventory.InventStatus;
                existingInventory.InventTotal = inventory.InventTotal;

                UpdateInventory(existingInventory);
            }

            await SaveAsync();
            return true;
        }

        /// <summary>
        /// Retorna todos os inventários associados a um InventGuid específico, criados no último ano (365 dias).
        /// </summary>
        /// <param name="inventGuid">O GUID do inventário principal a ser filtrado.</param>
        /// <returns>Uma lista de objetos Inventory criados no último ano.</returns>
        public async Task<List<Inventory>> GetRecentInventoriesByGuid(string inventGuid)
        {
            var oneYearAgo = DateTime.Now.AddDays(-365);

            var inventories = await _context.Inventory
                .Where(i => i.InventGuid == inventGuid)
                .Where(i => i.InventCreated.HasValue && i.InventCreated.Value >= oneYearAgo)
                .OrderByDescending(i => i.InventStatus == InventoryStatus.Iniciado.ToString())
                .ThenByDescending(i => i.InventCreated)
                .ToListAsync();

            return inventories;
        }

        public async Task<List<Inventory>> GetInventoryAllAsync()
        {
            var oneYearAgo = DateTime.Now.AddDays(-365);

            var inventories = await _context.Inventory
                .Where(i => i.InventCreated.HasValue && i.InventCreated.Value >= oneYearAgo)
                .OrderByDescending(i => i.InventStatus == InventoryStatus.Iniciado.ToString())
                .ThenByDescending(i => i.InventCreated)

                .ToListAsync();

            return inventories;
        }


        public async Task<bool> DeleteInventoryAsync(string InventCode)
        {
            var inventory = await GetInventoryByGuidAsync(InventCode);

            if (inventory == null)
                return false;

            // Regra de negócio: Deletar todos os records filhos (limpeza)
            var records = await GetRecordsByInventCodeAsync(inventory.InventCode);
            foreach (var record in records)
            {
                DeleteRecord(record);
            }

            DeleteInventory(inventory);
            await SaveAsync();
            return true;
        }

        // --- InventoryRecord - Lógica de Service ---

        public async Task<(int created, int updated)> CreateOrUpdateInventoryRecordsAsync(List<InventoryRecordRequest> batchRequests)
        {
            var recordsToCreate = new List<InventoryRecord>();
            var recordsToUpdate = new List<InventoryRecord>();

            // Extraímos os InventCodes únicos de todos os lotes para atualizar os totais depois
            var affectedInventCodes = batchRequests.Select(r => r.InventCode).Distinct().ToList();

            // --- 1. PREPARAÇÃO DOS DADOS ---
            foreach (var batch in batchRequests)
            {
                // Validação de Regra de Negócio: O Inventário pai (Inventory) deve existir para este lote
                var parentInventoryExists = await GetInventoryByCodeAsync(batch.InventCode);
                if (parentInventoryExists == null)
                {
                    throw new KeyNotFoundException($"Inventário principal com código '{batch.InventCode}' não encontrado.");
                }

                foreach (var record in batch.Records)
                {
                    // Forçamos o InventCode do registro a ser o mesmo do lote pai (Batch)
                    record.InventCode = batch.InventCode;

                    if (string.IsNullOrWhiteSpace(record.InventCode) || string.IsNullOrWhiteSpace(record.InventProduct))
                    {
                        throw new ArgumentException($"Registro inválido: 'InventCode' e 'InventProduct' são obrigatórios.");
                    }

                    // Tenta encontrar registro existente pela chave única composta
                    var existingRecord = await GetRecordByUniqueKeysAsync(
                        record.InventCode,
                        record.InventLocation,
                        record.InventBarcode);

                    if (existingRecord != null)
                    {
                        // ATUALIZAÇÃO (Mapeamos os novos valores para a instância rastreada pelo EF)
                        existingRecord.InventUser = record.InventUser;
                        existingRecord.InventUnitizer = record.InventUnitizer;
                        existingRecord.InventProduct = record.InventProduct;
                        existingRecord.InventStandardStack = record.InventStandardStack;
                        existingRecord.InventQtdStack = record.InventQtdStack;
                        existingRecord.InventQtdIndividual = record.InventQtdIndividual;
                        existingRecord.InventTotal = record.InventTotal;
                        existingRecord.InventCreated = record.InventCreated ?? DateTime.Now;

                        recordsToUpdate.Add(existingRecord);
                    }
                    else
                    {
                        // INSERÇÃO
                        // Garantimos que a data de criação não seja nula
                        record.InventCreated ??= DateTime.Now;
                        recordsToCreate.Add(record);
                    }
                }
            }

            // --- 2. SALVAMENTO DOS RECORDS ---
            try
            {
                if (recordsToUpdate.Any())
                {
                    UpdateRangeRecords(recordsToUpdate);
                }

                if (recordsToCreate.Any())
                {
                    AddRangeRecords(recordsToCreate);
                }

                await SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                var dbErrorMessage = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidOperationException($"Erro ao salvar/atualizar registros (Database). Detalhes: {dbErrorMessage}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro inesperado ao salvar registros: {ex.Message}", ex);
            }

            // --- 3. ATUALIZAÇÃO DE TOTAIS ---
            try
            {
                foreach (var inventCode in affectedInventCodes)
                {
                    await UpdateParentInventoryTotalAsync(inventCode);
                }

                await SaveAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao atualizar totais do Inventário pai: {ex.Message}", ex);
            }

            return (recordsToCreate.Count, recordsToUpdate.Count);
        }

        public async Task<bool> DeleteInventoryRecordAsync(int _inventId)
        {
            var record = await GetRecordByIdAsync(_inventId);

            if (record == null)
                return false;

            var inventCode = record.InventCode;

            DeleteRecord(record);
            await SaveAsync();

            // Recalcula o total do Inventário pai após exclusão
            await UpdateParentInventoryTotalAsync(inventCode);
            await SaveAsync();

            return true;
        }

        public async Task<bool> DeleteInvRecByCodeItemAsync(string inventCode, string unitizer, string location, string item)
        {
            var record = await _context.InventoryRecord.FirstOrDefaultAsync(r =>
                r.InventCode == inventCode &&
                r.InventUnitizer == unitizer &&
                r.InventLocation == location &&
                r.InventProduct == item);

            if (record == null)
                return false;

            DeleteRecord(record);
            await SaveAsync();
            await UpdateParentInventoryTotalAsync(inventCode);
            await SaveAsync();
            return true;
        }

        // ---------------------------- Método para sincronização ----------------------------
        public async Task<IEnumerable<object>> GetProductsPagedAsync(int pageNumber, int pageSize = 10000)
        {
            // O Skip calcula quantos registros devem ser ignorados com base na página
            // Ex: Página 1 -> pula 0. Página 2 -> pula 10.000.
            return await _context.Product
                .OrderBy(p => p.ProductId) // Ordenação é obrigatória para usar Skip/Take
                .Select(p => new
                {
                    p.ProductId,
                    p.Barcode,
                    p.ProductName,
                    p.Status,
                    p.SalesChannel
                })
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetProductCountAsync()
        {
            // Retorna a contagem total de registros na tabela Product
            return await _context.Product.CountAsync();
        }

        // ---------------------------- Método para sincronização ----------------------------

        private async Task UpdateParentInventoryTotalAsync(string inventCode)
        {
            decimal newTotal = await CalculateInventoryTotalAsync(inventCode);

            var inventoryToUpdate = await GetInventoryByCodeAsync(inventCode);

            if (inventoryToUpdate != null)
            {
                inventoryToUpdate.InventTotal = newTotal;
                UpdateInventory(inventoryToUpdate);
            }
        }

    }
}