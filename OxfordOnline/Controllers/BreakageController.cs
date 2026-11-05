using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OxfordOnline.Models.Dto;
using OxfordOnline.Resources;
using OxfordOnline.Services;

namespace OxfordOnline.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class BreakageController : ControllerBase
    {
        private readonly BreakageService _breakageService;
        private readonly ILogger<BreakageController> _logger;

        public BreakageController(BreakageService breakageService, ILogger<BreakageController> logger)
        {
            _breakageService = breakageService;
            _logger = logger;
        }

        /// <summary>
        /// GET: v1/Breakage
        /// Lista os registros de quebra criados nos últimos 3 meses, do mais recente para o mais antigo.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetRecentBreakages()
        {
            try
            {
                var records = await _breakageService.GetRecentBreakagesAsync();
                return Ok(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, EndPointsMessages.ErrorGettingBreakages);
                return StatusCode(500, new
                {
                    message = EndPointsMessages.ErrorGettingBreakages,
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        /// <summary>
        /// POST: v1/Breakage
        /// Insere um registro de quebra (sem BreakageId) ou atualiza um existente (com BreakageId).
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateBreakage([FromBody] BreakageRecordRequest request)
        {
            try
            {
                var (record, created) = await _breakageService.CreateOrUpdateBreakageAsync(request);

                return StatusCode(created ? StatusCodes.Status201Created : StatusCodes.Status200OK, new
                {
                    message = created ? EndPointsMessages.SucessSave : EndPointsMessages.UpdateOk,
                    data = record
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BreakageConflictException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, EndPointsMessages.LogErrorSavingBreakage);
                return StatusCode(500, new
                {
                    message = EndPointsMessages.ErrorSave,
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        /// <summary>
        /// DELETE: v1/Breakage/{breakageId}
        /// Exclui um registro de quebra. Registros com status BAIXADO não podem ser excluídos.
        /// </summary>
        [Authorize]
        [HttpDelete("{breakageId}")]
        public async Task<IActionResult> DeleteBreakage(uint breakageId)
        {
            try
            {
                var deleted = await _breakageService.DeleteBreakageAsync(breakageId);
                if (!deleted)
                    return NotFound(new { message = EndPointsMessages.BreakageNotFound });

                return Ok(new { message = EndPointsMessages.DeleteOk });
            }
            catch (BreakageConflictException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, EndPointsMessages.LogErrorDeletingBreakage, breakageId);
                return StatusCode(500, new
                {
                    message = EndPointsMessages.ErrorDel,
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
    }
}
