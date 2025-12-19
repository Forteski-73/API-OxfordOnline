using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OxfordOnline.Models;
using OxfordOnline.Services;
using OxfordOnline.Resources;

namespace OxfordOnline.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class OxfordController : ControllerBase
    {
        private readonly OxfordService _oxfordService;
        private readonly ILogger<OxfordController> _logger;

        public OxfordController(OxfordService oxfordService, ILogger<OxfordController> logger)
        {
            _oxfordService = oxfordService;
            _logger = logger;
        }

        // GET: /Oxford
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Oxford>>> GetOxfords()
        {
            var list = await _oxfordService.GetOxfordListByProductIdsAsync(new List<string>());
            return Ok(list);
        }

        // GET: /Oxford/{productId}
        [Authorize]
        [HttpGet("{productId}")]
        public async Task<ActionResult<Oxford>> GetOxfordByProductId(string productId)
        {
            var oxford = await _oxfordService.GetOxfordByProductIdAsync(productId);
            if (oxford == null)
                return NotFound(new { message = EndPointsMessages.ProductNotFound });

            return Ok(oxford);
        }

        // POST: /Oxford
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateOxfords([FromBody] List<Oxford> oxfords)
        {
            if (oxfords == null || oxfords.Count == 0)
                return BadRequest(new { message = EndPointsMessages.InvalidProductList });

            try
            {
                await _oxfordService.CreateOrUpdateOxfordsAsync(oxfords);
                return Ok(new { message = string.Format(EndPointsMessages.ProductSavedSuccess, oxfords.Count) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, EndPointsMessages.LogErrorSavingProduct);
                return StatusCode(500, new
                {
                    message = EndPointsMessages.ErrorSavingProducts,
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // PUT: /Oxford/{productId}
        [Authorize]
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateOxford(string productId, [FromBody] Oxford oxford)
        {
            if (oxford == null || productId != oxford.ProductId)
                return BadRequest(new { message = EndPointsMessages.InvalidProductData });

            try
            {
                var existing = await _oxfordService.GetOxfordByProductIdAsync(productId);
                if (existing == null)
                    return NotFound(new { message = EndPointsMessages.ProductNotFoundForUpdate });

                await _oxfordService.UpdateOxfordAsync(oxford);
                return Ok(oxford);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, EndPointsMessages.LogErrorUpdatingProduct, productId);
                return StatusCode(500, new
                {
                    message = EndPointsMessages.ErrorUpdatingProduct,
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // DELETE: /Oxford/{productId}
        [Authorize]
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteOxford(string productId)
        {
            try
            {
                var success = await _oxfordService.DeleteOxfordAsync(productId);
                if (!success)
                    return NotFound(new { message = EndPointsMessages.ProductNotFoundForDelete });

                return Ok(new { message = EndPointsMessages.ProductDeletedSuccess });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, EndPointsMessages.LogErrorDeletingProduct, productId);
                return StatusCode(500, new
                {
                    message = EndPointsMessages.ErrorDeletingProduct,
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
        // POST: /Oxford/Products
        [Authorize]
        [HttpPost("Products")]
        public async Task<ActionResult<List<ProductData>>> PostFilterOxfordByFields([FromBody] OxfordFilterRequest filter)
        {
            if (filter == null)
                return BadRequest(new { message = "Filtro inválido." });

            var result = await _oxfordService.GetFilterOxfordByFields(filter);

            if (result == null)
                return NotFound(new { message = "Nenhum produto encontrado com os filtros fornecidos." });

            return Ok(result);
        }
    }
}
