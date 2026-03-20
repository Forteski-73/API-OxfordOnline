using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("product_pack_item")]
    public class ProductPackItem
    {
        [Column("pack_id")]
        public int PackId { get; set; }

        [Required]
        [Column("pack_item")]
        [MaxLength(10)]
        public string PackItem { get; set; } = string.Empty;

        [Required]
        [Column("pack_user")]
        [StringLength(100)]
        public string PackUser { get; set; }

        // Navigation
        [ForeignKey("PackId")]
        public ProductPack? ProductPack { get; set; }

        // Relacionamento com Product futuramente:
        [ForeignKey("PackItem")]
        public Product? Product { get; set; }
    }
}