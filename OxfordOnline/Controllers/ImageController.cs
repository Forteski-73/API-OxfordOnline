using OxfordOnline.Data;
using OxfordOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OxfordOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ImageController(AppDbContext context)
        {
            _context = context;
        }

        // POST: Criar múltiplas imagens
        [HttpPost]
        public async Task<IActionResult> CreateImages([FromBody] List<Image> images)
        {
            if (images == null || !images.Any())
                return BadRequest("Nenhuma imagem foi enviada.");

            if (images.Any(img => img.ItemId <= 0 || string.IsNullOrWhiteSpace(img.Path)))
                return BadRequest("Todas as imagens devem ter um ItemId válido e um caminho (Path).");

            // Verifica se todos os ItemId existem
            var itemIds = images.Select(i => i.ItemId).Distinct();
            var existingItemIds = await _context.Item
                .Where(i => itemIds.Contains(i.Id))
                .Select(i => i.Id)
                .ToListAsync();

            var invalidItemIds = itemIds.Except(existingItemIds).ToList();
            if (invalidItemIds.Any())
                return NotFound($"Itens não encontrados: {string.Join(", ", invalidItemIds)}");

            // Atualiza datas e remove referência ao objeto Item
            foreach (var img in images)
            {
                img.UpdateDate = DateTime.UtcNow;
                img.Item = null;
            }

            _context.Image.AddRange(images);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"{images.Count} imagem(ns) adicionada(s) com sucesso!",
                imagens = images
            });
        }

        // GET: Todas as imagens
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Image>>> GetAllImages()
        {
            var images = await _context.Image.ToListAsync();
            return Ok(images);
        }

        // GET: Imagem por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Image>> GetImageById(int id)
        {
            var image = await _context.Image.FindAsync(id);
            if (image == null)
                return NotFound("Imagem não encontrada.");

            return Ok(image);
        }

        // GET: Imagens por item
    }
}