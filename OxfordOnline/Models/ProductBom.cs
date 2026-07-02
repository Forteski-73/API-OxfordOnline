using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("product_bom")]
    public class ProductBom
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("product_id")]
        [StringLength(10)]
        public string ProductId { get; set; }

        [Column("product_bom_id")]
        [StringLength(10)]
        public string? ProductBomId { get; set; }

        [Column("product_name")]
        [StringLength(255)]
        public string? ProductName { get; set; }

        [Required]
        [Column("product_qty")]
        public int ProductQty { get; set; } = 1;
    }
}