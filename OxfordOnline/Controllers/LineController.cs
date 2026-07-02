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
    public class LinesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LinesController(AppDbContext context)
        {
            _context = context;
        }

        // POST: Inserir ou atualizar linhas
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpsertLines([FromBody] List<ProductLine> lines)
        {
            if (lines == null || !lines.Any())
            {
                return BadRequest("Nenhuma linha foi enviada.");
            }

            try
            {
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line.LineId))
                    {
                        return BadRequest("Dados da linha inválidos. Todos os itens precisam de um LinesId.");
                    }

                    var existingLine = await _context.ProductLine.FindAsync(line.LineId);

                    if (existingLine == null)
                    {
                        // Insere se não existir
                        _context.ProductLine.Add(line);
                    }
                    else
                    {
                        // Atualiza se já existir
                        _context.ProductLine.Update(line);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = $"{lines.Count} linha(s) salva(s) com sucesso." });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "Erro interno não especificado.";
                return StatusCode(500, $"Erro ao salvar a linha: {ex.Message} | Inner: {innerMessage}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        // GET: Todas as linhas de produto
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductLine>>> GetAllLines()
        {
            var lines = await _context.ProductLine.ToListAsync();
            return Ok(lines);
        }

        // GET: Obter linhas por ID da marca
        [Authorize]
        [HttpGet("ByBrand/{brandId}")]
        public async Task<ActionResult<IEnumerable<ProductLine>>> GetLinesByBrandId([FromRoute] string brandId)
        {
            // Valida o parâmetro
            if (string.IsNullOrWhiteSpace(brandId))
            {
                return BadRequest("O ID da marca é obrigatório.");
            }

            // Realiza o join para buscar as linhas únicas
            var lines = await _context.ProductAttributeMap
                // Filtra pelo BrandId
                .Where(map => map.BrandId == brandId)
                // Inclui a propriedade de navegação 'Line' para carregar os dados completos
                .Include(map => map.Line)
                // Seleciona a propriedade de navegação 'Line'
                .Select(map => map.Line)
                // Remove duplicatas para retornar apenas linhas únicas
                .Distinct()
                // Garante que o resultado não seja nulo (embora a consulta já evite isso)
                .Where(line => line != null)
                // Converte para uma lista
                .ToListAsync();

            if (!lines.Any())
            {
                return NotFound("Nenhuma linha encontrada para esta marca.");
            }

            return Ok(lines);
        }
    }
}