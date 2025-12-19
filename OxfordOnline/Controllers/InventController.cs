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
    public class InventController : ControllerBase
    {
        private readonly InventService _inventService;
        private readonly ILogger<InventController> _logger;

        public InventController(InventService inventService, ILogger<InventController> logger)
        {
            _inventService = inventService;
            _logger = logger;
        }

        // GET: /Invent
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invent>>> GetInvents()
        {
            var list = await _inventService.GetInventListByProductIdsAsync(new List<string>()); // retorna vazio por padrão
            return Ok(list);
        }

        // GET: /Invent/{productId}
        [Authorize]
        [HttpGet("{productId}")]
        public async Task<ActionResult<Invent>> GetInventByProductId(string productId)
        {
            var invent = await _inventService.GetInventByProductIdAsync(productId);
            if (invent == null)
                return NotFound(new { message = EndPointsMessages.ProductNotFound });

            return Ok(invent);
        }

        // POST: /Invent
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateInvents([FromBody] List<Invent> invents)
        {
            if (invents == null || invents.Count == 0)
                return BadRequest(new { message = EndPointsMessages.InvalidProductList });

            try
            {
                await _inventService.CreateOrUpdateInventsAsync(invents);
                return Ok(new { message = string.Format(EndPointsMessages.ProductSavedSuccess, invents.Count) });
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

        // PUT: /Invent/{productId}
        [Authorize]
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateInvent(string productId, [FromBody] Invent invent)
        {
            if (invent == null || productId != invent.ProductId)
                return BadRequest(new { message = EndPointsMessages.InvalidProductData });

            try
            {
                var existing = await _inventService.GetInventByProductIdAsync(productId);
                if (existing == null)
                    return NotFound(new { message = EndPointsMessages.ProductNotFoundForUpdate });

                await _inventService.UpdateInventAsync(invent);
                return Ok(invent);
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

        // DELETE: /Invent/{productId}
        [Authorize]
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteInvent(string productId)
        {
            try
            {
                var success = await _inventService.DeleteInventAsync(productId);
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
    }
}