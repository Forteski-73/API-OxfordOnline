using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    namespace OxfordOnline.Models
    {
        [Table("product_packing_bom_ax")]
        public class ProductPackingBomAx
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Required]
            [Column("product_id")]
            [StringLength(10)]
            public string ProductId { get; set; }

            [Column("product_packing_bom_id")]
            [StringLength(10)]
            public string? ProductPackingBomId { get; set; }

            [Column("product_name")]
            [StringLength(255)]
            public string? ProductName { get; set; }

            [Required]
            [Column("product_qty")]
            public int ProductQty { get; set; } = 1;
        }
    }
}
