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
        public async Task<ActionResult<IEnumerable<ProductPack>>> GetAllPacks()
        {
            var packs = await _packingService.GetAllPacksAsync();
            return Ok(packs);
        }

        // GET: /v1/ProductPacking/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductPack>> GetPackById(int id)
        {
            var pack = await _packingService.GetPackByIdAsync(id);
            if (pack == null)
                return NotFound(new { message = EndPointsMessages.NotFound });

            return Ok(pack);
        }

        // POST: /v1/ProductPacking
        [HttpPost]
        public async Task<ActionResult<ProductPack>> CreatePack([FromBody] ProductPack pack)
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
        public async Task<IActionResult> UpdatePack(int id, [FromBody] ProductPack pack)
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

        // DELETE: /v1/ProductPacking/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePack(int id)
        {
            try
            {
                var success = await _packingService.DeletePackAsync(id);
                if (!success)
                    return NotFound(new { message = EndPointsMessages.ProductNotFoundForDelete });

                return Ok(new { message = EndPointsMessages.ProductDeletedSuccess });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar pacote {PackId}", id);
                return StatusCode(500, new { message = EndPointsMessages.ErrorDeletingProduct, error = ex.Message });
            }
        }

        // GET: /v1/ProductPacking/ByProduct/{productId}
        [HttpGet("ByProduct/{productId}")]
        public async Task<ActionResult<IEnumerable<ProductPack>>> GetPacksByProduct(string productId)
        {
            var packs = await _packingService.GetPacksByProductAsync(productId);
            return Ok(packs);
        }


        // --- Endpoints de Imagens ---

        // GET: /v1/ProductPacking/Images/{packId}
        [HttpGet("Images/{packId}")]
        public async Task<ActionResult<IEnumerable<ProductPackImage>>> GetImagesByPack(int packId)
        {
            var images = await _packingService.GetImagesByPackAsync(packId);
            if (images == null || !images.Any())
                return NotFound(new { message = EndPointsMessages.NotFound });

            return Ok(images);
        }

        // POST: /v1/ProductPacking/Images
        [HttpPost("Images")]
        public async Task<ActionResult<ProductPackImage>> AddImageToPack([FromBody] ProductPackImage image)
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
    }
}