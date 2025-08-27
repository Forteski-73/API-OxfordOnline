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
    }
}
