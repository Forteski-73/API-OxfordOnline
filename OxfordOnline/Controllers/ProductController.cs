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

        // GET: api/product/ProductAll
        [HttpGet("ProductAll")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsWithImagesAndTags(
            [FromQuery] string? ItemId = null,
            [FromQuery] string? ItemBarCode = null,
            [FromQuery] string? Name = null)
        {
            IQueryable<Product> query = _context.Product;

            if (!string.IsNullOrWhiteSpace(ItemId))
            {
                query = query.Where(p => p.ItemId == ItemId);
            }
            if (!string.IsNullOrWhiteSpace(ItemBarCode))
            {
                query = query.Where(p => p.ItemBarCode != null && p.ItemBarCode.StartsWith(ItemBarCode));
            }
            if (!string.IsNullOrWhiteSpace(Name))
            {
                query = query.Where(p => p.Name != null && p.Name.Contains(Name));
            }

            var products = await query
                .Take(10) // top 10 primeiros encontrados
                .Select(p => new ProductDto
                {
                    ItemId = p.ItemId,
                    ItemBarCode = p.ItemBarCode,
                    ProdBrandId = p.ProdBrandId,
                    ProdBrandDescriptionId = p.ProdBrandDescriptionId,
                    ProdLinesId = p.ProdLinesId,
                    ProdLinesDescriptionId = p.ProdLinesDescriptionId,
                    ProdDecorationId = p.ProdDecorationId,
                    ProdDecorationDescriptionId = p.ProdDecorationDescriptionId,
                    Name = p.Name,
                    UnitVolumeML = p.UnitVolumeML,
                    ItemNetWeight = p.ItemNetWeight,
                    ProdFamilyId = p.ProdFamilyId,
                    ProdFamilyDescriptionId = p.ProdFamilyDescriptionId,
                    GrossWeight = p.GrossWeight,
                    TaraWeight = p.TaraWeight,
                    GrossDepth = p.GrossDepth,
                    GrossWidth = p.GrossWidth,
                    GrossHeight = p.GrossHeight,
                    NrOfItems = p.NrOfItems,
                    TaxFiscalClassification = p.TaxFiscalClassification,

                    ProductImages = _context.Image
                        .Where(img => img.ProductId == p.ItemId)
                        .OrderBy(img => img.Sequence)
                        .Select(img => new ImageDto
                        {
                            Id = img.Id,
                            Path = img.Path,
                            Sequence = img.Sequence
                        }).ToList(),

                    ProductTags = _context.Tag
                        .Where(tag => tag.ProductId == p.ItemId)
                        .Select(tag => new TagDto
                        {
                            Id = tag.Id,
                            ValueTag = tag.ValueTag
                        }).ToList()
                })
                .ToListAsync();

            return Ok(products);
        }
    }

    // DTOs usados para retornar dados estruturados no JSON
    public class ProductDto //DTO significa Data Transfer Object
    {
        public string ItemId { get; set; } = string.Empty;
        public string? ItemBarCode { get; set; }
        public string? ProdBrandId { get; set; }
        public string? ProdBrandDescriptionId { get; set; }
        public string? ProdLinesId { get; set; }
        public string? ProdLinesDescriptionId { get; set; }
        public string? ProdDecorationId { get; set; }
        public string? ProdDecorationDescriptionId { get; set; }
        public string? Name { get; set; }
        public decimal? UnitVolumeML { get; set; }
        public decimal? ItemNetWeight { get; set; }
        public string? ProdFamilyId { get; set; }
        public string? ProdFamilyDescriptionId { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? TaraWeight { get; set; }
        public decimal? GrossDepth { get; set; }
        public decimal? GrossWidth { get; set; }
        public decimal? GrossHeight { get; set; }
        public decimal? NrOfItems { get; set; }
        public string? TaxFiscalClassification { get; set; }

        public List<ImageDto> ProductImages { get; set; } = new();
        public List<TagDto> ProductTags { get; set; } = new();
    }

    public class ImageDto
    {
        public int? Id { get; set; }
        public string Path { get; set; } = string.Empty;
        public int? Sequence { get; set; }
    }

    public class TagDto
    {
        public int? Id { get; set; }
        public string ValueTag { get; set; } = string.Empty;
    }

}
