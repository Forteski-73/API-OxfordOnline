using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models;
using System.Linq;

namespace OxfordOnline.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class ProductAttributeMapController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductAttributeMapController(AppDbContext context)
        {
            _context = context;
        }

        // POST: Inserir ou atualizar mapeamentos
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpsertProductAttributeMaps([FromBody] List<ProductAttributeMap> maps)
        {
            if (maps == null || !maps.Any())
            {
                return BadRequest("Nenhum mapeamento foi enviado.");
            }

            try
            {
                foreach (var map in maps)
                {
                    if (string.IsNullOrWhiteSpace(map.BrandId) || string.IsNullOrWhiteSpace(map.LineId) || string.IsNullOrWhiteSpace(map.DecorationId))
                    {
                        return BadRequest("Dados de mapeamento inválidos. Todos os itens precisam de Marca, Linha e Decoração.");
                    }

                    // Verifica se o registro já existe com base na chave composta
                    var existingMap = await _context.ProductAttributeMap
                        .FirstOrDefaultAsync(m => m.BrandId == map.BrandId && m.LineId == map.LineId && m.DecorationId == map.DecorationId);

                    if (existingMap == null)
                    {
                        // Insere se não existir
                        _context.ProductAttributeMap.Add(map);
                    }
                    else
                    {
                        // Atualiza a data de atualização se já existir.
                        existingMap.CreatedAt = DateTime.Now;
                        _context.ProductAttributeMap.Update(existingMap);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = $"{maps.Count} mapeamento(s) salvo(s) com sucesso." });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "Erro interno não especificado.";
                return StatusCode(500, $"Erro ao salvar o mapeamento: {ex.Message} | Inner: {innerMessage}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        // GET: Mapeamentos
        // Recebe BrandId, LineId e/ou DecorationId como parâmetros opcionais na query string
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductAttributeMap>>> GetProductAttributeMaps(
            [FromQuery] string? brandId,
            [FromQuery] string? lineId,
            [FromQuery] string? decorationId)
        {
            var query = _context.ProductAttributeMap.AsQueryable();

            if (!string.IsNullOrWhiteSpace(brandId))
            {
                query = query.Where(m => m.BrandId == brandId);
            }

            if (!string.IsNullOrWhiteSpace(lineId))
            {
                query = query.Where(m => m.LineId == lineId);
            }

            if (!string.IsNullOrWhiteSpace(decorationId))
            {
                query = query.Where(m => m.DecorationId == decorationId);
            }

            var maps = await query.ToListAsync();

            if (!maps.Any())
            {
                return NotFound("Nenhum mapeamento encontrado com os critérios fornecidos.");
            }

            return Ok(maps);
        }
    }
}