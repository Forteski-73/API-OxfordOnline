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
    public class DecorationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DecorationController(AppDbContext context)
        {
            _context = context;
        }

        // POST: Inserir ou atualizar decorações
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpsertDecorations([FromBody] List<ProductDecoration> decorations)
        {
            if (decorations == null || !decorations.Any())
            {
                return BadRequest("Nenhuma decoração foi enviada.");
            }

            try
            {
                foreach (var decoration in decorations)
                {
                    if (string.IsNullOrWhiteSpace(decoration.DecorationId))
                    {
                        return BadRequest("Dados da decoração inválidos. Todos os itens precisam de um DecorationId.");
                    }

                    var existingDecoration = await _context.ProductDecoration.FindAsync(decoration.DecorationId);

                    if (existingDecoration == null)
                    {
                        // Insere se não existir
                        _context.ProductDecoration.Add(decoration);
                    }
                    else
                    {
                        // Atualiza se já existir
                        _context.ProductDecoration.Update(decoration);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = $"{decorations.Count} decoração(ões) salva(s) com sucesso." });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "Erro interno não especificado.";
                return StatusCode(500, $"Erro ao salvar a decoração: {ex.Message} | Inner: {innerMessage}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        // GET: Todas as decorações
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDecoration>>> GetAllDecorations()
        {
            var decorations = await _context.ProductDecoration.ToListAsync();
            return Ok(decorations);
        }

        // GET: Obter decorações por ID da marca e linha
        [Authorize]
        [HttpGet("ByBrandLine/{brandId}/{lineId}")]
        public async Task<ActionResult<IEnumerable<ProductDecoration>>> GetDecorationsByBrandAndLine(
            [FromRoute] string brandId,
            [FromRoute] string lineId)
        {
            // Valida os parâmetros
            if (string.IsNullOrWhiteSpace(brandId) || string.IsNullOrWhiteSpace(lineId))
            {
                return BadRequest("Os IDs de marca e linha são obrigatórios.");
            }

            // Realiza o join implícito para buscar as decorações únicas
            var decorations = await _context.ProductAttributeMap
                // Filtra pelo BrandId E pelo LineId para garantir a hierarquia
                .Where(map => map.BrandId == brandId && map.LineId == lineId)
                // Inclui a propriedade de navegação 'Decoration'
                .Include(map => map.Decoration)
                // Seleciona a propriedade de navegação 'Decoration'
                .Select(map => map.Decoration)
                // Remove duplicatas para retornar apenas decorações únicas
                .Distinct()
                // Garante que o resultado não seja nulo
                .Where(decoration => decoration != null)
                // Converte para uma lista
                .ToListAsync();

            if (!decorations.Any())
            {
                return NotFound("Nenhuma decoração encontrada para esta marca e linha.");
            }

            return Ok(decorations);
        }
    }
}
