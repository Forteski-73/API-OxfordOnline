using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OxfordOnline.Models
{
    [Table("product_invent_dim")]
    public class InventDim
    {
        [Key]
        [Column("id")]
        [JsonIgnore]
        public int Id { get; set; }

        [Required]
        [Column("product_id")]
        [MaxLength(50)]
        public string ProductId { get; set; } = string.Empty;

        [Column("location_id")]
        public string? LocationId { get; set; }

        [Column("company_id")]
        public string? CompanyId { get; set; }

        [Column("quantity")]
        public int? Quantity { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }

        // Opcional: relacionamento com a tabela Product
        //[ForeignKey("ProductId")]
        //public Product? Product { get; set; }
    }
}
