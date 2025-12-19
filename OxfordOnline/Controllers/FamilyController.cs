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
    public class FamilyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FamilyController(AppDbContext context)
        {
            _context = context;
        }

        // POST: Inserir ou atualizar
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpsertFamilies([FromBody] List<ProductFamily> families)
        {
            if (families == null || !families.Any())
            {
                return BadRequest("Nenhuma família foi enviada.");
            }

            try
            {
                foreach (var family in families)
                {
                    if (string.IsNullOrWhiteSpace(family.FamilyId))
                    {
                        return BadRequest("Dados da família inválidos. Todos os itens precisam de um FamilyId.");
                    }

                    var existingFamily = await _context.ProductFamily.FindAsync(family.FamilyId);

                    if (existingFamily == null)
                    {
                        // Insere se não existir
                        _context.ProductFamily.Add(family);
                    }
                    else
                    {
                        // Atualiza se já existir
                        _context.ProductFamily.Update(family);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = $"{families.Count} família(s) salva(s) com sucesso." });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "Erro interno não especificado.";
                return StatusCode(500, $"Erro ao salvar a família: {ex.Message} | Inner: {innerMessage}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        // GET: Todas as famílias
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductFamily>>> GetAllFamilies()
        {
            var families = await _context.ProductFamily.ToListAsync();
            return Ok(families);
        }
    }
}