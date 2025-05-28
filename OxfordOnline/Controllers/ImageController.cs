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

        [HttpPost]
        public async Task<IActionResult> CreateImages([FromBody] List<Image> images)
        {
            if (images == null || !images.Any())
                return BadRequest("Nenhuma imagem foi enviada.");

            // Validação: ProductId deve estar preenchido e Path não pode ser vazio
            if (images.Any(img => string.IsNullOrWhiteSpace(img.ProductId) || string.IsNullOrWhiteSpace(img.Path)))
                return BadRequest("Todas as imagens devem ter um ProductId válido e um caminho (Path).");

            // Verifica se todos os ProductId existem
            var productIds = images.Select(i => i.ProductId).Distinct().ToList();
            var existingProductIds = await _context.Product
                .Where(p => productIds.Contains(p.ItemId))
                .Select(p => p.ItemId)
                .ToListAsync();

            var invalidProductIds = productIds.Except(existingProductIds).ToList();
            if (invalidProductIds.Any())
                return NotFound($"Produtos não encontrados: {string.Join(", ", invalidProductIds)}");

            // Deleta todas as imagens existentes para os ProductIds enviados
            var imagesToRemove = await _context.Image
                .Where(img => productIds.Contains(img.ProductId))
                .ToListAsync();

            if (imagesToRemove.Any())
            {
                _context.Image.RemoveRange(imagesToRemove);
                await _context.SaveChangesAsync(); // Apaga antes de inserir
            }

            // Remove tracking da entidade Product
            foreach (var img in images)
            {
                img.Product = null;
            }

            // Insere novas imagens
            _context.Image.AddRange(images);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"{images.Count} imagem(ns) adicionada(s) com sucesso!",
                imagens = images
            });
        }

        /*
        // POST: Criar múltiplas imagens
        [HttpPost]
        public async Task<IActionResult> CreateImages([FromBody] List<Image> images)
        {
            if (images == null || !images.Any())
                return BadRequest("Nenhuma imagem foi enviada.");

            // Validação: ProductId deve estar preenchido e Path não pode ser vazio
            if (images.Any(img => string.IsNullOrWhiteSpace(img.ProductId) || string.IsNullOrWhiteSpace(img.Path)))
                return BadRequest("Todas as imagens devem ter um ProductId válido e um caminho (Path).");

            // Verifica se todos os ProductId existem
            var productIds = images.Select(i => i.ProductId).Distinct();
            var existingProductIds = await _context.Product
                .Where(p => productIds.Contains(p.ItemId))
                .Select(p => p.ItemId)
                .ToListAsync();

            var invalidProductIds = productIds.Except(existingProductIds).ToList();
            if (invalidProductIds.Any())
                return NotFound($"Produtos não encontrados: {string.Join(", ", invalidProductIds)}");

            // Atualiza datas e remove referência ao objeto Product
            foreach (var img in images)
            {
                img.Product = null;  // evitar tracking de entidades relacionadas neste ponto
            }

            _context.Image.AddRange(images);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"{images.Count} imagem(ns) adicionada(s) com sucesso!",
                imagens = images
            });
        }
        */

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

        // GET: Imagens por ProductId
        [HttpGet("Product/{productId}")]
        public async Task<ActionResult<IEnumerable<Image>>> GetImagesByProductId(string productId)
        {
            var images = await _context.Image
                .Where(i => i.ProductId == productId)
                .ToListAsync();

            if (images == null || images.Count == 0)
                return NotFound("Nenhuma imagem encontrada para o produto informado.");

            return Ok(images);
        }
    }
}