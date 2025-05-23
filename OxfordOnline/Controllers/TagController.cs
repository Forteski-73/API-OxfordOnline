using OxfordOnline.Data;
using OxfordOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OxfordOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TagController(AppDbContext context)
        {
            _context = context;
        }

        // POST: Criar múltiplas tags
        [HttpPost]
        public async Task<IActionResult> CreateTags([FromBody] List<Tag> tags)
        {
            if (tags == null || !tags.Any())
                return BadRequest("Nenhuma tag foi enviada.");

            // Validação: ProductId e Tag devem estar preenchidos
            if (tags.Any(tag => string.IsNullOrWhiteSpace(tag.ProductId) || string.IsNullOrWhiteSpace(tag.ValueTag)))
                return BadRequest("Todas as tags devem ter um ProductId e uma tag (Value) válidos.");

            // Verifica se todos os ProductIds existem
            var productIds = tags.Select(t => t.ProductId).Distinct();
            var existingProductIds = await _context.Product
                .Where(p => productIds.Contains(p.ItemId))
                .Select(p => p.ItemId)
                .ToListAsync();

            var invalidProductIds = productIds.Except(existingProductIds).ToList();
            if (invalidProductIds.Any())
                return NotFound($"Produtos não encontrados: {string.Join(", ", invalidProductIds)}");

            // Remove relação para evitar problemas de tracking
            foreach (var tag in tags)
            {
                tag.Product = null;
            }

            try
            {
                // Remove todas as tags existentes do produtos
                var tagsToRemove = await _context.Tag
                    .Where(t => productIds.Contains(t.ProductId))
                    .ToListAsync();

                if (tagsToRemove.Any())
                {
                    _context.Tag.RemoveRange(tagsToRemove);
                }

                // Insere as novas tags
                _context.Tag.AddRange(tags);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"{tags.Count} tag(s) adicionada(s) com sucesso, e {tagsToRemove.Count} removida(s).",
                    tags = tags
                });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "Erro interno não especificado.";
                return StatusCode(500, $"Erro ao salvar tags: {ex.Message} | Inner: {innerMessage}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        // GET: Todas as tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetAllTags()
        {
            var tags = await _context.Tag.ToListAsync();
            return Ok(tags);
        }

        // GET: Tag por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetTagById(int id)
        {
            var tag = await _context.Tag.FindAsync(id);
            if (tag == null)
                return NotFound("Tag não encontrada.");

            return Ok(tag);
        }

        // GET: Tags por ProductId
        [HttpGet("Product/{productId}")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTagsByProductId(string productId)
        {
            var tags = await _context.Tag
                .Where(t => t.ProductId == productId)
                .ToListAsync();

            if (tags == null || tags.Count == 0)
                return NotFound("Nenhuma tag encontrada para o produto informado.");

            return Ok(tags);
        }
    }
}
