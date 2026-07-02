using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Resources;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
                return BadRequest(EndPointsMessages.NotFound);
            }

            try
            {
                foreach (var pallet in pallets)
                {
                    var existingPallet = await _context.Pallet.FindAsync(pallet.PalletId);

                    if (existingPallet != null)
                    {
                        existingPallet.TotalQuantity = pallet.TotalQuantity;
                        existingPallet.Status = pallet.Status;
                        existingPallet.Location = pallet.Location;
                        existingPallet.UpdatedUser = pallet.UpdatedUser;
                        existingPallet.ImagePath = pallet.ImagePath;
                    }
                    else
                    {
                        _context.Pallet.Add(pallet);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = EndPointsMessages.SucessSave });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "Erro interno não especificado.";
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", innerMessage));
            }
            catch (Exception ex)
            {
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", ex.Message));
            }
        }

        // GET: Todas os paletes
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pallet>>> GetAllPallets()
        {
            var pallets = await _context.Pallet
                .Where(p => p.Status != "R") // MENOS OS RECEBIDOS
                .ToListAsync();

            return Ok(pallets);
        }

        [Authorize]
        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<Pallet>>> GetFilterPallet(
            [FromQuery] string? status,
            [FromQuery] string? txtFilter)
        {
            var query = _context.Pallet.AsQueryable();

            string? dbStatus = null;
            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToUpperInvariant())
                {
                    case "INICIADO": dbStatus = "I"; break;
                    case "MONTADO":  dbStatus = "M"; break;
                    case "RECEBIDO": dbStatus = "R"; break;
                }
            }
            if (!string.IsNullOrEmpty(dbStatus))
            {
                query = query.Where(p => p.Status == dbStatus);
            }

            if (!string.IsNullOrWhiteSpace(txtFilter))
            {
                var filterText = txtFilter.Trim().ToLower();

                int filterPalletId;
                bool isNumericPalletId = int.TryParse(txtFilter.Trim(), out filterPalletId);

                query = query.Where(p =>
                    (isNumericPalletId && p.PalletId == filterPalletId) ||
                    ((p.Location ?? "").ToLower().Contains(filterText)) ||
                    ((p.CreatedUser ?? "").ToLower() == filterText)
                );
            }

            var pallets = await query.ToListAsync();
            return Ok(pallets);
        }

        [Authorize]
        [HttpGet("SearchItem")]
        public async Task<ActionResult<IEnumerable<PalletItem>>> GetFilterPalletItem(
        [FromQuery] string? status,
        [FromQuery] string? txtFilter)
        {
            var query = _context.PalletItem
            .Include(pi => pi.Product)
            .AsQueryable();

            string? dbStatus = null;
            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToUpperInvariant())
                {
                    case "INICIADO": dbStatus = "I"; break;
                    case "MONTADO":  dbStatus = "M"; break;
                    case "RECEBIDO": dbStatus = "R"; break;
                }
            }
            if (!string.IsNullOrEmpty(dbStatus))
            {
                query = query.Where(p => p.Status == dbStatus);
            }

            if (!string.IsNullOrWhiteSpace(txtFilter))
            {
                var filterText = txtFilter.Trim().ToLower();

                int filterPalletId = 0;
                bool isNumericPalletId = int.TryParse(txtFilter.Trim(), out filterPalletId);

                query = query.Where(p =>
                    (isNumericPalletId && p.PalletId == filterPalletId) ||
                    (p.ProductId.ToUpper() == filterText.ToUpper()) ||
                    (p.Product!.ProductName!.ToUpper().Contains(filterText)) ||
                    ((p.UserId.ToUpper() ?? "").ToLower() == filterText)
                );
            }

            var items = await query.ToListAsync();

            items.ForEach(item => {
                if (item.Product != null)
                {
                    item.ProductName = item.Product.ProductName;
                }
                item.Product = null;
            });

            return Ok(items);
        }

        // POST: Inserir ou atualizar itens de palete
        [Authorize]
        [HttpPost("Item")]
        public async Task<IActionResult> UpsertPalletItems([FromBody] List<PalletItem> palletItems)
        {
            if (palletItems == null || !palletItems.Any())
            {
                return BadRequest(EndPointsMessages.NotFound);
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
                        existingItem.Quantity           = item.Quantity;
                        existingItem.QuantityReceived   = item.QuantityReceived;
                        existingItem.UserId             = item.UserId;
                        existingItem.Status             = item.Status;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok( new { message = EndPointsMessages.SucessSave });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "Erro interno não especificado.";
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", innerMessage));
            }
            catch (Exception ex)
            {
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", ex.Message));
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
                    return NotFound(EndPointsMessages.NotFound);
                }

                var items = await _context.PalletItem
                    .Include(pi => pi.Product)
                    .Where(pi => pi.PalletId == palletId)
                    .ToListAsync();

                items.ForEach(item => {
                    if (item.Product != null)
                    {
                        item.ProductName = item.Product.ProductName;
                    }
                    item.Product = null;
                });

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", ex.Message));
            }
        }

        // DELETE: Excluir um palete (e todos os seus itens via CASCADE no banco de dados)
        [Authorize]
        [HttpDelete("{palletId}")]
        public async Task<IActionResult> DeletePallet(int palletId)
        {
            try
            {
                var palletToDelete = await _context.Pallet.FindAsync(palletId);

                if (palletToDelete == null)
                {
                    return NotFound(EndPointsMessages.NotFound);
                }

                _context.Pallet.Remove(palletToDelete);

                await _context.SaveChangesAsync();

                return Ok(new { message = EndPointsMessages.DeleteOk });
            }
            catch (Exception ex)
            {
                var baseError = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", baseError));
            }
        }

        // DELETE: Excluir um item específico do palete
        [Authorize]
        [HttpDelete("Item/{palletId}/{productId}")]
        public async Task<IActionResult> DeletePalletItem(int palletId, string productId)
        {
            try
            {
                var itemToDelete = await _context.PalletItem
                    .FirstOrDefaultAsync(pi => pi.PalletId == palletId && pi.ProductId == productId);

                if (itemToDelete == null)
                {
                    return NotFound(EndPointsMessages.NotFound);
                }

                _context.PalletItem.Remove(itemToDelete);
                await _context.SaveChangesAsync();

                return Ok(new { message = EndPointsMessages.DeleteOk});
            }
            catch (Exception ex)
            {
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", ex.Message));
            }
        }

        /// GET: Todos os itens de todos os paletes
        [Authorize]
        [HttpGet("AllItems")]
        public async Task<ActionResult<IEnumerable<PalletItem>>> GetAllPalletItems()
        {
            try
            {
                var items = await _context.PalletItem
                    .Include(pi => pi.Product)
                    .ToListAsync();

                items.ForEach(item => {
                    if (item.Product != null)
                    {
                        item.ProductName = item.Product.ProductName;
                    }
                    item.Product = null;
                });

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", ex.Message));
            }
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------

        // GET: Recuperar todas as imagens de um PalletImage
        /// <summary>
        /// Recupera todas as imagens (PalletImage) associadas a um PalletId específico.
        /// </summary>
        /// <param name="palletId">O ID do palete.</param>
        /// <returns>Uma lista de objetos PalletImage.</returns>
        [Authorize]
        [HttpGet("Image/{palletId}")]
        public async Task<ActionResult<IEnumerable<PalletImage>>> GetPalletImages(int palletId)
        {
            try
            {
                var images = await _context.PalletImage
                    .Where(pi => pi.PalletId == palletId)
                    .ToListAsync();

                if (!images.Any())
                {
                    return Ok(new List<PalletImage>());
                }

                return Ok(images);
            }
            catch (Exception ex)
            {
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", ex.Message));
            }
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------

        // POST: Deletar imagens existentes e inserir novas imagens de um Pallet
        /// <summary>
        /// Deleta todas as imagens existentes para o PalletId e insere uma nova lista de imagens.
        /// Essa operação é feita de forma atômica (transacional).
        /// </summary>
        /// <param name="palletImages">Lista de objetos PalletImage a serem inseridos.</param>
        /// <returns>Status de sucesso ou erro.</returns>
        [Authorize]
        [HttpPost("Image")]
        public async Task<IActionResult> ReplacePalletImages([FromBody] List<PalletImage> palletImages)
        {
            if (palletImages == null || !palletImages.Any())
            {
                return BadRequest(EndPointsMessages.ImageNull);
            }

            var palletId = palletImages.First().PalletId;

            foreach (var image in palletImages)
            {
                image.Id = 0;
                if (image.PalletId != palletId)
                {
                    return BadRequest(EndPointsMessages.ImagesOnPalletNotOk);
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.PalletImage
                    .Where(pi => pi.PalletId == palletId)
                    .ExecuteDeleteAsync();

                _context.PalletImage.AddRange(palletImages);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = EndPointsMessages.UpdateOk });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"{EndPointsMessages.ErrorUp} {palletId}: {innerMessage}");
            }
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------

        // DELETE: Excluir todas as imagens de um Pallet
        /// <summary>
        /// Exclui todos os registros de imagens (PalletImage) associados a um PalletId específico.
        /// </summary>
        /// <param name="palletId">O ID do palete cujas imagens devem ser excluídas.</param>
        /// <returns>Status de sucesso ou erro.</returns>
        /// 
        [Authorize]
        [HttpDelete("Image/{palletId}")] // Rota: DELETE v1/Pallet/Image/{palletId}
        public async Task<IActionResult> DeletePalletImages(int palletId)
        {
            try
            {
                var imagesToDelete = await _context.PalletImage
                    .Where(pi => pi.PalletId == palletId)
                    .ToListAsync();

                if (!imagesToDelete.Any())
                {
                    return Ok(new { message = EndPointsMessages.ImageNull });
                }

                _context.PalletImage.RemoveRange(imagesToDelete);

                var count = await _context.SaveChangesAsync();

                return Ok(new { message = EndPointsMessages.DeleteOk });
            }
            catch (Exception ex)
            {
                var baseError = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"{EndPointsMessages.ErrorDel} {palletId}: {baseError}");
            }
        }

        // PUT: Atualizar apenas o status do palete
        /// <summary>
        /// Atualiza o campo Status de um palete específico.
        /// Rota: PUT v1/Pallet/{palletId}/status
        /// </summary>
        /// <param name="palletId">O ID do palete a ser atualizado.</param>
        /// <param name="updateDto">Objeto contendo o novo status e o usuário de atualização.</param>
        /// <returns>Status de sucesso ou erro.</returns>
        [Authorize]
        [HttpPut("Status")] // Rota: Ex: v1/Pallet/status
        public async Task<IActionResult> UpdatePalletStatus([FromBody] PalletStatusUpdateRequest updateRequest)
        {
            // Verifica se o PalletId é válido e se o Status foi fornecido
            if (updateRequest.PalletId <= 0 || string.IsNullOrWhiteSpace(updateRequest.Status))
            {
                return BadRequest(EndPointsMessages.NotFound);
            }

            // Transação para garantir que o Pallet e todos os seus Itens sejam atualizados de forma atômica.
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Encontra e atualiza o Pallet principal
                var existingPallet = await _context.Pallet.FindAsync(updateRequest.PalletId);

                if (existingPallet == null)
                {
                    await transaction.RollbackAsync(); // Reverte a transação (não tenha feito nada ainda)
                    return NotFound(EndPointsMessages.NotFound);
                }

                // Atualiza os campos do Pallet
                existingPallet.Status = updateRequest.Status;

                if (!string.IsNullOrWhiteSpace(updateRequest.UpdatedUser))
                {
                    existingPallet.UpdatedUser = updateRequest.UpdatedUser;
                }

                // Salva a alteração no Pallet
                await _context.SaveChangesAsync();

                // Atualiza os itens do pallet
                // ExecuteUpdateAsync é mais eficiente, pois gera uma única instrução SQL UPDATE.
                var itemsUpdated = await _context.PalletItem
                    .Where(pi => pi.PalletId == updateRequest.PalletId)
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(pi => pi.Status, updateRequest.Status)
                        .SetProperty(pi => pi.UserId, updateRequest.UpdatedUser)
                    );

                // Confirma a transação
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = EndPointsMessages.UpdateOk
                    //itemsAffected = itemsUpdated
                });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                var innerMessage = ex.InnerException?.Message ?? EndPointsMessages.ErrorUp;
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", innerMessage));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Reverte as alterações em caso de erro geral
                return StatusCode(500, EndPointsMessages.Error.Replace("%Error%", ex.Message));
            }
        }
    }
}