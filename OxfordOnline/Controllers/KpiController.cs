using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OxfordOnline.Models.Dto;
using OxfordOnline.Services;

namespace OxfordOnline.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class KpiController : ControllerBase
    {
        private readonly KpiService _kpiService;
        private readonly ILogger<KpiController> _logger;

        public KpiController(KpiService kpiService, ILogger<KpiController> logger)
        {
            _kpiService = kpiService;
            _logger = logger;
        }

        /// <summary>
        /// GET: v1/Kpi/Completude
        /// Retorna a completude de cadastro de imagens por categoria (Produto, Embalagem,
        /// Paletização), considerando apenas produtos com status ativo.
        /// </summary>
        [Authorize]
        [HttpGet("Completude")]
        public async Task<ActionResult<KpiCompletudeResult>> GetCompletude()
        {
            try
            {
                var result = await _kpiService.GetCompletudeAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular KPI de completude.");
                return StatusCode(500, new
                {
                    message = "Erro ao calcular KPI de completude.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
    }
}
