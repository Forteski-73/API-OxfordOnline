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
    public class InventDimController : ControllerBase
    {
        private readonly InventDimService _inventDimService;
        private readonly ILogger<InventDimController> _logger;

        public InventDimController(InventDimService inventDimService, ILogger<InventDimController> logger)
        {
            _inventDimService = inventDimService;
            _logger = logger;
        }

        // GET: /InventDim/{productId}?locationId=LOC1&companyId=COM1
        [Authorize]
        [HttpGet("{productId}")]
        public async Task<ActionResult<InventDim>> GetInventDim(string productId, [FromQuery] string? locationId, [FromQuery] string? companyId)
        {
            var invent = await _inventDimService.GetInventDimAsync(productId, locationId, companyId);
            if (invent == null)
                return NotFound(new { message = EndPointsMessages.ProductNotFound });

            return Ok(invent);
        }

        // GET: /InventDim/list/{productId}
        [Authorize]
        [HttpGet("List/{productId}")]
        public async Task<ActionResult<List<InventDim>>> GetInventDimsByProductId(string productId)
        {
            var list = await _inventDimService.GetInventDimsByProductIdAsync(productId);
            return Ok(list);
        }

        // POST: /InventDim
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateInventDims([FromBody] List<InventDim> inventDims)
        {
            if (inventDims == null || inventDims.Count == 0)
                return BadRequest(new { message = EndPointsMessages.InvalidProductList });

            try
            {
                await _inventDimService.CreateOrUpdateInventDimsAsync(inventDims);
                return Ok(new { message = string.Format(EndPointsMessages.ProductSavedSuccess, inventDims.Count) });
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

        // PUT: /InventDim
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateInventDim([FromBody] InventDim inventDim)
        {
            if (inventDim == null)
                return BadRequest(new { message = EndPointsMessages.InvalidProductData });

            try
            {
                var existing = await _inventDimService.GetInventDimAsync(inventDim.ProductId, inventDim.LocationId, inventDim.CompanyId);
                if (existing == null)
                    return NotFound(new { message = EndPointsMessages.ProductNotFoundForUpdate });

                await _inventDimService.UpdateInventDimAsync(inventDim);
                return Ok(inventDim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, EndPointsMessages.LogErrorUpdatingProduct, inventDim.ProductId);
                return StatusCode(500, new
                {
                    message = EndPointsMessages.ErrorUpdatingProduct,
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // DELETE: /InventDim/{productId}?locationId=LOC1&companyId=COM1
        [Authorize]
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteInventDim(string productId, [FromQuery] string? locationId, [FromQuery] string? companyId)
        {
            try
            {
                var success = await _inventDimService.DeleteInventDimAsync(productId, locationId, companyId);
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