using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace OxfordOnline.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService _inventoryService;

        public InventoryController(InventoryService inventoryService)
        {
            // O InventoryService deve ser injetado e gerenciado pelo container DI
            _inventoryService = inventoryService;
        }

        // -----------------------------------------------------------------------------------------------------------------
        // --- MÉTODOS PARA InventoryGuid (tabela 'inventory_guid') ---
        // -----------------------------------------------------------------------------------------------------------------

        // POST: Criar um novo GUID de inventário (ou confirmar existência)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateInventoryGuid([FromBody] InventoryGuid inventoryGuid)
        {
            if (inventoryGuid == null || string.IsNullOrWhiteSpace(inventoryGuid.InventGuid))
                return BadRequest("O campo 'InventGuid' é obrigatório e deve ser enviado na requisição.");

            try
            {
                // A lógica de idempotência (verificar se existe e inserir se não) está no serviço.
                var created = await _inventoryService.CreateInventoryGuidAsync(inventoryGuid);

                if (created)
                {
                    // Retorna 201 Created (sucesso)
                    return CreatedAtAction(nameof(GetInventoryGuidByGuid), new { inventGuid = inventoryGuid.InventGuid }, inventoryGuid);
                }
                else
                {
                    // Se não foi criado (já existia), buscamos o registro e retornamos 200 OK
                    var existingRecord = await _inventoryService.GetGuidByInventGuidAsync(inventoryGuid.InventGuid);

                    return Ok(new
                    {
                        message = $"O GUID '{inventoryGuid.InventGuid}' já existe no inventário. Nenhuma alteração foi feita.",
                        data = existingRecord
                    });
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Erros de DbUpdate/Internal Server serão tratados de forma genérica
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        // GET: Todos os GUIDs de inventário
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryGuid>>> GetAllInventoryGuids()
        {
            var guids = await _inventoryService.GetAllInventoryGuidsAsync();

            if (!guids.Any())
                return NotFound("Nenhum GUID de inventário encontrado.");

            return Ok(guids);
        }

        // GET: GUID de inventário por invent_guid (a string GUID)
        [HttpGet("ByGuid/{inventGuid}")]
        public async Task<ActionResult<InventoryGuid>> GetInventoryGuidByGuid(string inventGuid)
        {
            var inventoryGuid = await _inventoryService.GetGuidByInventGuidAsync(inventGuid);

            if (inventoryGuid == null)
                return NotFound("GUID de inventário não encontrado.");

            return Ok(inventoryGuid);
        }

        // -----------------------------------------------------------------------------------------------------------------
        // --- MÉTODOS PARA INVENTORY (tabela 'inventory') ---
        // -----------------------------------------------------------------------------------------------------------------

        [Authorize]
        [HttpPost("Inventory")]
        public async Task<IActionResult> CreateOrUpdateInventory([FromBody] Inventory inventory)
        {
            if (inventory == null)
                return BadRequest("Dados de inventário inválidos.");

            try
            {
                // A lógica de validação de GUID, Insert/Update está no serviço.
                var result = await _inventoryService.CreateOrUpdateInventoryAsync(inventory);

                if (!string.IsNullOrWhiteSpace(inventory.InventCode))
                {
                    // Atualização
                    return Ok(new
                    {
                        message = $"Inventário com ID {inventory.InventCode} atualizado com sucesso.",
                        data = inventory
                    });
                }
                else
                {
                    // Criação (assumindo que o serviço preencheu o InventCode se for nova criação)
                    return CreatedAtAction(
                        nameof(GetInventoryByGuidInventCode),
                        new { guid = inventory.InventGuid, inventCode = inventory.InventCode },
                        inventory
                    );
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado ao salvar/atualizar inventário: {ex.Message}");
            }
        }

        // GET: Inventário por ID
        [HttpGet("Inventory/{guid}")]
        public async Task<ActionResult<Inventory>> GetInventoryByGuid(string guid)
        {
            var inventory = await _inventoryService.GetInventoryByGuidAsync(guid);

            if (inventory == null)
                return NotFound("Inventário não encontrado pelo ID.");

            return Ok(inventory);
        }

        [HttpGet("Inventory/{guid}/{inventCode}")]
        public async Task<ActionResult<Inventory>> GetInventoryByGuidInventCode(string guid, string inventory)
        {
            var inventoryData = await _inventoryService.GetInventoryByGuidInventCodeAsync(guid, inventory);

            if (inventoryData == null)
                return NotFound("Inventário não encontrado pelo ID.");

            return Ok(inventoryData);
        }

        // DELETE: Excluir Inventário por ID
        [Authorize]
        [HttpDelete("Inventory/{inventCode}")]
        public async Task<IActionResult> DeleteInventory(string inventCode)
        {
            try
            {
                // A lógica de deleção (que também deleta os Records filhos) está no serviço.
                var deleted = await _inventoryService.DeleteInventoryAsync(inventCode);

                if (!deleted)
                    return NotFound("Inventário não encontrado para exclusão.");

                return Ok(new { message = $"Inventário com ID {inventCode} excluído com sucesso (e seus registros filhos)." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado ao excluir inventário: {ex.Message}");
            }
        }

        // -----------------------------------------------------------------------------------------------------------------
        // --------- MÉTODOS PARA INVENTORY RECORD (tabela 'inventory_record') ---------------------------------------------
        // -----------------------------------------------------------------------------------------------------------------

        // Rota para Inserir ou Atualizar em BATCH InventoryRecord.
        [Authorize]
        [HttpPost("Record")]
        public async Task<IActionResult> CreateOrUpdateInventoryRecord([FromBody] List<InventoryRecordRequest> records)
        {
            String Msg = string.Empty;
            if (records == null || !records.Any())
                return BadRequest("Nenhum registro de inventário foi enviado.");

            try
            {
                // A lógica de validação, Insert/Update em lote, e recalculo do total está no serviço.
                var (created, updated) = await _inventoryService.CreateOrUpdateInventoryRecordsAsync(records);


                var updatedInventory = await _inventoryService
                    .GetInventoryByGuidInventCodeAsync(
                        records.First().InventGuid,
                        records.First().InventCode
                    );


                if (created > 0)
                {
                    Msg = "CONTAGEM INSERIDA COM SUCESSO!";
                }
                if (updated > 0)
                {
                    Msg = "CONTAGEM ATUALIZADA COM SUCESSO!";
                }
                var response = new InventoryRecordResponse
                {
                    InventGuid = updatedInventory.InventGuid,
                    InventCode = updatedInventory.InventCode,
                    InventTotal = updatedInventory.InventTotal,
                    Message = Msg
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado ao salvar/atualizar o registro de inventário: {ex.Message}");
            }
        }

        // GET: Todos os InventoryRecords para um dado InventCode
        [HttpGet("Record/ByCode/{inventCode}")]
        public async Task<ActionResult<IEnumerable<InventoryRecord>>> GetInventoryRecordsByInventCode(string inventCode)
        {
            var records = await _inventoryService.GetRecordsByInventCodeAsync(inventCode);

            if (!records.Any())
                return NotFound($"Nenhum registro de inventário encontrado para o código '{inventCode}'.");

            return Ok(records);
        }

        // GET: InventoryRecord por ID
        [HttpGet("Record/{id}")]
        public async Task<ActionResult<InventoryRecord>> GetInventoryRecordById(int _inventId)
        {
            var record = await _inventoryService.GetRecordByIdAsync(_inventId);

            if (record == null)
                return NotFound("Registro de inventário não encontrado pelo ID.");

            return Ok(record);
        }

        // GET: Todos os Inventários do último ano para um dado InventGuid.
        // Rota: GET v1/Inventory/RecentByGuid/{inventGuid}
        [HttpGet("RecentByGuid/{inventGuid}")]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetRecentInventoriesByGuid(string inventGuid)
        {
            // O service deve retornar uma lista de Inventory, mas sua assinatura estava 
            // incorreta. Vou assumir que ela foi corrigida para Task<List<Inventory>>
            // para funcionar corretamente aqui.

            var inventories = await _inventoryService.GetRecentInventoriesByGuid(inventGuid);

            if (inventories == null || !inventories.Any())
            {
                return NotFound($"Nenhum inventário encontrado para o GUID '{inventGuid}' no último ano.");
            }

            return Ok(inventories);
        }

        // DELETE: Excluir InventoryRecord por ID
        [Authorize]
        [HttpDelete("Record/{id}")]
        public async Task<IActionResult> DeleteInventoryRecord(int id)
        {
            try
            {
                // A lógica de exclusão e recalculo do total do pai está no serviço.
                var deleted = await _inventoryService.DeleteInventoryRecordAsync(id);

                if (!deleted)
                    return NotFound("Registro de inventário não encontrado para exclusão.");

                return Ok(new { message = $"Registro de inventário com ID {id} excluído com sucesso e o total do inventário pai foi atualizado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado ao excluir registro de inventário: {ex.Message}");
            }
        }

        // -----------------------------------------------------------------------------------------------------------------
        // --- MÉTODOS PARA SICRONIZAR PRODUCT (tabela 'product') ---
        // -----------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// GET: v1/Inventory/Product?page=1
        /// Retorna uma lista paginada de produtos (10.000 por vez).
        /// </summary>
        [HttpGet("Product")]
        public async Task<IActionResult> GetProductsPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10000)
        {
            try
            {
                // Validações básicas para evitar valores negativos ou zero
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10; // Define um mínimo
                if (pageSize > 30000) pageSize = 30000; // Opcional: Define um limite máximo por segurança

                var products = await _inventoryService.GetProductsPagedAsync(page, pageSize);

                if (products == null || !products.Any())
                    return NotFound(new { message = "Fim da lista de produtos ou página sem registros." });

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao recuperar lote de produtos: {ex.Message}");
            }
        }

        /// <summary>
        /// GET: v1/Inventory/Product/Count
        /// Retorna o total de registros na tabela de produtos para controle do cliente.
        /// </summary>
        [HttpGet("Product/Count")]
        public async Task<IActionResult> GetProductCount()
        {
            try
            {
                var total = await _inventoryService.GetProductCountAsync();
                return Ok(new { total });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter contagem de produtos: {ex.Message}");
            }
        }


        // -----------------------------------------------------------------------------------------------------------------
        // --- MÉTODOS PARA INVENTORY MASK (tabela 'inventory_mask') ---
        // -----------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// GET: v1/Inventory/Masks
        /// Retorna todas as máscaras de campo cadastradas no banco de dados.
        /// </summary>
        [HttpGet("Masks")]
        public async Task<ActionResult<IEnumerable<InventoryMask>>> GetAllInventoryMasks()
        {
            try
            {
                var masks = await _inventoryService.GetAllInventoryMasksAsync();

                if (masks == null || !masks.Any())
                    return NotFound("Nenhuma máscara de inventário encontrada.");

                return Ok(masks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao recuperar máscaras: {ex.Message}");
            }
        }

    }
}