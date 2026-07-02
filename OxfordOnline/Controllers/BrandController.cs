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
    public class BrandController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BrandController(AppDbContext context)
        {
            _context = context;
        }

        // POST: Inserir ou atualizar
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpsertBrands([FromBody] List<ProductBrand> brands)
        {
            if (brands == null || !brands.Any())
            {
                return BadRequest("Nenhuma marca foi enviada.");
            }

            try
            {
                foreach (var brand in brands)
                {
                    if (string.IsNullOrWhiteSpace(brand.BrandId))
                    {
                        return BadRequest("Dados da marca inválidos. Todos os itens precisam de um BrandId.");
                    }

                    var existingBrand = await _context.ProductBrand.FindAsync(brand.BrandId);

                    if (existingBrand == null)
                    {
                        // Insere se não existir
                        _context.ProductBrand.Add(brand);
                    }
                    else
                    {
                        // Atualiza se já existir
                        _context.ProductBrand.Update(brand);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = $"{brands.Count} marca(s) salva(s) com sucesso." });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "Erro interno não especificado.";
                return StatusCode(500, $"Erro ao salvar a marca: {ex.Message} | Inner: {innerMessage}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        // GET: Todas as marcas
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductBrand>>> GetAllBrands()
        {
            var brands = await _context.ProductBrand.ToListAsync();
            return Ok(brands);
        }
    }
}