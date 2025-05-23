using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OxfordOnline.Models
{
    [Table("product_tag")]
    public class Tag
    {
        [Key]
        [Column("id")]
        public int? Id { get; set; }

        [Required]
        [Column("tag", TypeName = "text")]
        public string ValueTag { get; set; } = string.Empty;

        [Required]
        [Column("product_id")]
        [MaxLength(20)]
        public string ProductId { get; set; } = string.Empty;

        // Navegação para Product
        [ForeignKey(nameof(ProductId))]
        public virtual Product? Product { get; set; }
    }
}
