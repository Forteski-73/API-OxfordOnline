using OxfordOnline.Data;
using OxfordOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OxfordOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItemController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Retorna todos os itens
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItens()
        {
            var itens = await _context.Item.ToListAsync();
            return Ok(itens);
        }

        // GET por ID: Retorna um único item pelo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItemPorId(int id)
        {
            var item = await _context.Item.FindAsync(id);

            if (item == null)
            {
                return NotFound(new { message = "Item não encontrado!" });
            }

            return Ok(item);
        }

        // POST: Criar um item 
        [HttpPost]
        public async Task<IActionResult> CriarItem([FromBody] Item item)
        {
            if (item == null)
                return BadRequest(new { mensagem = "Os dados do item são inválidos." });

            try
            {
                _context.Item.Add(item);            // Adiciona o item ao banco de dados
                await _context.SaveChangesAsync();

                // Retorna o item criado com status 201 Created
                return CreatedAtAction(nameof(CriarItem), new { id = item.Id }, item);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro ao salvar no banco de dados.",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Ocorreu um erro inesperado ao processar a solicitação.",
                    erro = ex.Message
                });
            }
        }
    }
}
