using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Resources;
using OxfordOnline.Services;
using System.IO.Compression;

namespace OxfordOnline.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class ProductPackingController : ControllerBase
    {
        private readonly ProductPackingService _packingService;
        private readonly ILogger<ProductPackingController> _logger;

        public ProductPackingController(ProductPackingService packingService, ILogger<ProductPackingController> logger)
        {
            _packingService = packingService;
            _logger = logger;
        }

        // GET: /v1/ProductPacking
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PalletGroup>>> GetAllPacks()
        {
            var packs = await _packingService.GetAllPacksAsync();
            return Ok(packs);
        }

        // GET: /v1/ProductPacking/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PalletGroup>> GetPackById(int id)
        {
            var pack = await _packingService.GetPackByIdAsync(id);
            if (pack == null)
                return NotFound(new { message = EndPointsMessages.NotFound });

            return Ok(pack);
        }

        // POST: /v1/ProductPacking
        [HttpPost]
        public async Task<ActionResult<PalletGroup>> CreatePack([FromBody] PalletGroup pack)
        {
            if (pack == null)
                return BadRequest(new { message = EndPointsMessages.InvalidProductData });

            try
            {
                var createdPack = await _packingService.CreatePackAsync(pack);
                return CreatedAtAction(nameof(GetPackById), new { id = createdPack.PackId }, createdPack);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pacote de produto.");
                return StatusCode(500, new { message = EndPointsMessages.ErrorSavingProducts, error = ex.Message });
            }
        }

        // PUT: /v1/ProductPacking/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePack(int id, [FromBody] PalletGroup pack)
        {
            if (pack == null || id != pack.PackId)
                return BadRequest(new { message = EndPointsMessages.InvalidProductData });

            try
            {
                var updated = await _packingService.UpdatePackAsync(pack);
                if (!updated)
                    return NotFound(new { message = EndPointsMessages.ProductNotFoundForUpdate });

                return Ok(pack);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar pacote {PackId}", id);
                return StatusCode(500, new { message = EndPointsMessages.ErrorUpdatingProduct, error = ex.Message });
            }
        }

        // DELETE: /v1/ProductPacking/{PackId}
        [HttpDelete("{PackId}")]
        public async Task<IActionResult> DeletePack(int PackId)
        {
            try
            {
                var success = await _packingService.DeletePackAsync(PackId);
                if (!success)
                    return NotFound(new { message = EndPointsMessages.ProductNotFoundForDelete });

                return Ok(new { message = EndPointsMessages.ProductDeletedSuccess });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar pacote {PackId}", PackId);
                return StatusCode(500, new { message = EndPointsMessages.ErrorDeletingProduct, error = ex.Message });
            }
        }

        // GET: /v1/ProductPacking/ByProduct/{productId}
        [HttpGet("ByProduct/{productId}")]
        public async Task<ActionResult<IEnumerable<PalletGroup>>> GetPacksByProduct(string productId)
        {
            var packs = await _packingService.GetPacksByProductAsync(productId);
            return Ok(packs);
        }


        // --- Endpoints de Imagens ---

        // GET: /v1/ProductPacking/Images/{packId}
        [HttpGet("Images/{packId}")]
        public async Task<ActionResult<IEnumerable<PalletGroupImage>>> GetImagesByPack(int packId)
        {
            var images = await _packingService.GetImagesByPackAsync(packId);
            if (images == null || !images.Any())
                return NotFound(new { message = EndPointsMessages.NotFound });

            return Ok(images);
        }

        // POST: /v1/ProductPacking/Images
        [HttpPost("Images")]
        public async Task<ActionResult<PalletGroupImage>> AddImageToPack([FromBody] PalletGroupImage image)
        {
            if (image == null)
                return BadRequest(new { message = EndPointsMessages.InvalidProductData });

            try
            {
                var createdImage = await _packingService.AddImageAsync(image);
                return Ok(createdImage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar imagem ao pacote {PackId}", image.PackId);
                return StatusCode(500, new { message = "Erro ao salvar imagem.", error = ex.Message });
            }
        }

        // DELETE: /v1/ProductPacking/Images/{packId}/{sequence}
        [HttpDelete("Images/{packId}/{sequence}")]
        public async Task<IActionResult> DeleteImage(int packId, int sequence)
        {
            try
            {
                var success = await _packingService.DeleteImageAsync(packId, sequence);
                if (!success)
                    return NotFound(new { message = "Imagem não encontrada para exclusão." });

                return Ok(new { message = "Imagem removida com sucesso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar imagem {Sequence} do pacote {PackId}", sequence, packId);
                return StatusCode(500, new { message = "Erro ao deletar imagem.", error = ex.Message });
            }
        }

        // --- Endpoints de Itens (Tabela Filha) ---

        // GET: /v1/ProductPacking/Items/{packId}
        [HttpGet("Items/{packId}")]
        public async Task<ActionResult<IEnumerable<PalletGroupItem>>> GetItemsByPack(int packId)
        {
            var items = await _packingService.GetItemsByPackAsync(packId);
            //if (items == null || !items.Any())
            //    return NotFound(new { message = "Nenhum item encontrado para esta montagem." });

            return Ok(items);
        }

        // POST: /v1/ProductPacking/Items
        [HttpPost("Items")]
        public async Task<ActionResult<PalletGroupItem>> AddItemToPack([FromBody] PalletGroupItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.PackProductId))
                return BadRequest(new { message = EndPointsMessages.InvalidProductData });

            try
            {
                var createdItem = await _packingService.AddItemAsync(item);
                return Ok(createdItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar item {PackItem} ao pacote {PackId}", item.PackProductId, item.PackId);
                return StatusCode(500, new { message = "Erro ao salvar item da montagem.", error = ex.Message });
            }
        }

        // DELETE: /v1/ProductPacking/Items/{packId}/{sku}
        [HttpDelete("Items/{packId}/{sku}")]
        public async Task<IActionResult> DeleteItem(int packId, string sku)
        {
            try
            {
                // Note que 'sku' aqui refere-se à propriedade 'PackItem' da sua Model
                var success = await _packingService.DeleteItemAsync(packId, sku);
                if (!success)
                    return NotFound(new { message = "Item não encontrado para exclusão." });

                return Ok(new { message = "Item removido com sucesso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar item {Sku} do pacote {PackId}", sku, packId);
                return StatusCode(500, new { message = "Erro ao deletar item.", error = ex.Message });
            }
        }


        // --------------- Endpoints de BOM (product_packing_bom) ---------------

        // GET: /v1/ProductPacking/PackingBom/ByProduct/{productId}
        [HttpGet("PackingBom/ByProduct/{productId}")]
        public async Task<ActionResult<IEnumerable<ProductPackingBom>>> GetBomsByProduct(string productId)
        {
            var boms = await _packingService.GetBomsByProductAsync(productId);
            return Ok(boms);
        }


        // POST: /v1/ProductPacking/PackingBom
        [HttpPost("PackingBom")]
        public async Task<IActionResult> UpsertBom([FromBody] ProductPackingBomRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ProductId))
                return BadRequest(new { message = EndPointsMessages.InvalidProductData });

            try
            {
                var success = await _packingService.UpsertBomAsync(request);

                if (!success)
                    return BadRequest(new { message = "Não foi possível processar os dados da BOM." });

                return Ok(new { message = "Estrutura BOM salva com sucesso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar estrutura BOM para o produto {ProductId}", request.ProductId);
                return StatusCode(500, new { message = "Erro ao salvar a estrutura BOM.", error = ex.Message });
            }
        }

        // DELETE: /v1/ProductPacking/PackingBom/ByProduct/{productId}
        [HttpDelete("PackingBom/ByProduct/{productId}")]
        public async Task<IActionResult> DeleteAllBomsByProductId(string productId)
        {
            try
            {
                await _packingService.DeleteAllBomsByProductIdAsync(productId);
                return Ok(new { message = "Todas as estruturas BOM do produto foram removidas." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar estruturas BOM do produto {ProductId}", productId);
                return StatusCode(500, new { message = "Erro ao limpar estruturas BOM do produto.", error = ex.Message });
            }
        }
    }
}