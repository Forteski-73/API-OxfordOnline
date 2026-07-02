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
    public class TaxInformationController : ControllerBase
    {
        private readonly TaxInformationService _taxInformationService;
        private readonly ILogger<TaxInformationController> _logger;

        public TaxInformationController(TaxInformationService taxInformationService, ILogger<TaxInformationController> logger)
        {
            _taxInformationService = taxInformationService;
            _logger = logger;
        }

        // GET: /TaxInformation
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaxInformation>>> GetTaxInformations()
        {
            var list = await _taxInformationService.GetTaxInformationListByProductIdsAsync(new List<string>()); // vazio por padrão
            return Ok(list);
        }

        // GET: /TaxInformation/{productId}
        [Authorize]
        [HttpGet("{productId}")]
        public async Task<ActionResult<TaxInformation>> GetTaxInformationByProductId(string productId)
        {
            var taxInfo = await _taxInformationService.GetTaxInformationByProductIdAsync(productId);
            if (taxInfo == null)
                return NotFound(new { message = EndPointsMessages.ProductNotFound });

            return Ok(taxInfo);
        }

        // POST: /TaxInformation
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateTaxInformations([FromBody] List<TaxInformation> taxInfos)
        {
            if (taxInfos == null || taxInfos.Count == 0)
                return BadRequest(new { message = EndPointsMessages.InvalidProductList });

            try
            {
                await _taxInformationService.CreateOrUpdateTaxInformationsAsync(taxInfos);
                return Ok(new { message = string.Format(EndPointsMessages.ProductSavedSuccess, taxInfos.Count) });
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

        // PUT: /TaxInformation/{productId}
        [Authorize]
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateTaxInformation(string productId, [FromBody] TaxInformation taxInfo)
        {
            if (taxInfo == null || productId != taxInfo.ProductId)
                return BadRequest(new { message = EndPointsMessages.InvalidProductData });

            try
            {
                var existing = await _taxInformationService.GetTaxInformationByProductIdAsync(productId);
                if (existing == null)
                    return NotFound(new { message = EndPointsMessages.ProductNotFoundForUpdate });

                await _taxInformationService.UpdateTaxInformationAsync(taxInfo);
                return Ok(taxInfo);
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

        // DELETE: /TaxInformation/{productId}
        [Authorize]
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteTaxInformation(string productId)
        {
            try
            {
                var success = await _taxInformationService.DeleteTaxInformationAsync(productId);
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
