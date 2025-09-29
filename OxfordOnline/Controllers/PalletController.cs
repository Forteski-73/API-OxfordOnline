using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models;

namespace OxfordOnline.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class PalletController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PalletController(AppDbContext context)
        {
            _context = context;
        }

        // POST: Inserir ou atualizar
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpsertPallets([FromBody] List<Pallet> pallets)
        {
            if (pallets == null || !pallets.Any())
            {
                return BadRequest("Nenhum palete foi enviado.");
            }

            try
            {
                foreach (var pallet in pallets)
                {
                    if (pallet.PalletId <= 0)
                    {
                        // Insere se não tiver um ID válido (considerando IDs gerados automaticamente)
                        _context.Pallet.Add(pallet);
                    }
                    else
                    {
                        // Atualiza se já existir
                        var existingPallet = await _context.Pallet.FindAsync(pallet.PalletId);

                        if (existingPallet == null)
                        {
                            return NotFound($"Palete com ID {pallet.PalletId} não encontrado para atualização.");
                        }

                        // Atualiza as propriedades do objeto existente
                        existingPallet.TotalQuantity = pallet.TotalQuantity;
                        existingPallet.Status = pallet.Status;
                        existingPallet.Location = pallet.Location;
                        existingPallet.UpdatedUser = pallet.UpdatedUser;
                        existingPallet.ImagePath = pallet.ImagePath;
                        // created_at e created_user não são atualizados
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = $"{pallets.Count} palete(s) salvo(s) com sucesso." });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "Erro interno não especificado.";
                return StatusCode(500, $"Erro ao salvar o palete: {ex.Message} | Inner: {innerMessage}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        // GET: Todas os paletes
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pallet>>> GetAllPallets()
        {
            var pallets = await _context.Pallet.ToListAsync();
            return Ok(pallets);
        }

        // POST: Inserir ou atualizar itens de palete
        [Authorize]
        [HttpPost("Item")]
        public async Task<IActionResult> UpsertPalletItems([FromBody] List<PalletItem> palletItems)
        {
            if (palletItems == null || !palletItems.Any())
            {
                return BadRequest("Nenhum item de palete foi enviado.");
            }

            try
            {
                foreach (var item in palletItems)
                {
                    // A chave primária composta requer um método de busca mais complexo
                    // As chaves primárias são pallet_id e product_id
                    var existingItem = await _context.PalletItem
                        .FirstOrDefaultAsync(pi => pi.PalletId == item.PalletId && pi.ProductId == item.ProductId);

                    if (existingItem == null)
                    {
                        // Insere o novo item
                        _context.PalletItem.Add(item);
                    }
                    else
                    {
                        // Atualiza as propriedades do item existente
                        existingItem.Quantity = item.Quantity;
                        existingItem.UserId = item.UserId;
                        existingItem.Status = item.Status;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = $"{palletItems.Count} item(ns) de palete salvo(s) com sucesso." });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "Erro interno não especificado.";
                return StatusCode(500, $"Erro ao salvar os itens de palete: {ex.Message} | Inner: {innerMessage}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        // GET: Obter todos os itens de um palete
        [Authorize]
        [HttpGet("Item/{palletId}")]
        public async Task<ActionResult<IEnumerable<PalletItem>>> GetPalletItems(int palletId)
        {
            try
            {
                var palletExists = await _context.Pallet.AnyAsync(p => p.PalletId == palletId);
                if (!palletExists)
                {
                    return NotFound($"Palete com ID {palletId} não encontrado.");
                }

                var items = await _context.PalletItem
                    .Where(pi => pi.PalletId == palletId)
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar os itens do palete: {ex.Message}");
            }
        }

        // DELETE: Excluir um palete e todos os seus itens
        [Authorize]
        [HttpDelete("{palletId}")]
        public async Task<IActionResult> DeletePallet(int palletId)
        {
            try
            {
                // Busca o palete e seus itens para garantir que existem
                var palletToDelete = await _context.Pallet.FindAsync(palletId);
                if (palletToDelete == null)
                {
                    return NotFound($"Palete com ID {palletId} não encontrado.");
                }

                // Exclui os itens do palete primeiro
                var itemsToDelete = await _context.PalletItem.Where(pi => pi.PalletId == palletId).ToListAsync();
                if (itemsToDelete.Any())
                {
                    _context.PalletItem.RemoveRange(itemsToDelete);
                }

                // Exclui o palete
                _context.Pallet.Remove(palletToDelete);

                await _context.SaveChangesAsync();

                return Ok(new { message = $"Palete com ID {palletId} e todos os seus itens foram excluídos com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir o palete: {ex.Message}");
            }
        }

        // DELETE: Excluir um item específico do palete
        [Authorize]
        [HttpDelete("Item/{palletId}/{productId}")]
        public async Task<IActionResult> DeletePalletItem(int palletId, string productId)
        {
            try
            {
                // Busca o item específico do palete
                var itemToDelete = await _context.PalletItem
                    .FirstOrDefaultAsync(pi => pi.PalletId == palletId && pi.ProductId == productId);

                if (itemToDelete == null)
                {
                    return NotFound($"Item com ID de produto {productId} no palete {palletId} não encontrado.");
                }

                _context.PalletItem.Remove(itemToDelete);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Item com ID de produto {productId} foi excluído com sucesso do palete {palletId}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir o item do palete: {ex.Message}");
            }
        }
    }
}