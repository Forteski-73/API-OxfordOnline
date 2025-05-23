using OxfordOnline.Data;
using OxfordOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OxfordOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Product.ToListAsync();
            return Ok(products);
        }

        // GET: api/product/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(string id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Produto não encontrado!" });
            }
            return Ok(product);
        }

        // POST: api/product
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateProducts([FromBody] List<Product> products)
        {
            if (products == null || products.Count == 0)
                return BadRequest(new { message = "Lista de produtos inválida ou vazia." });

            try
            {
                foreach (var product in products)
                {
                    var existingProduct = await _context.Product.FindAsync(product.ItemId);

                    if (existingProduct != null)
                    {
                        // Atualiza os valores do produto existente
                        _context.Entry(existingProduct).CurrentValues.SetValues(product);
                    }
                    else
                    {
                        // Adiciona novo produto
                        _context.Product.Add(product);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = $"{products.Count} produto(s inserido(s) ou atualizado(s) com sucesso." });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro ao salvar no banco de dados.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro inesperado ao processar a solicitação.",
                    error = ex.Message
                });
            }
        }

        // PUT: api/product/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] Product product)
        {
            if (product == null || id != product.ItemId)
                return BadRequest(new { message = "Dados do produto inválidos ou ID não corresponde." });

            var existingProduct = await _context.Product.FindAsync(id);
            if (existingProduct == null)
                return NotFound(new { message = "Produto não encontrado para atualização." });

            // Atualiza os campos do produto existente
            _context.Entry(existingProduct).CurrentValues.SetValues(product);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(product);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro ao atualizar no banco de dados.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro inesperado ao processar a solicitação.",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/product/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Produto não encontrado para exclusão." });

            _context.Product.Remove(product);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Produto excluído com sucesso." });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro ao excluir no banco de dados.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro inesperado ao processar a solicitação.",
                    error = ex.Message
                });
            }
        }
    }
}
