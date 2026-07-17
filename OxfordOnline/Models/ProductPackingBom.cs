using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("product_packing_bom")]
    public class ProductPackingBom
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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

        [Required]
        [Column("product_seq")]
        public int ProductSeq { get; set; } = 1;

        [Column("updated_user")]
        [StringLength(50)]
        public string? UpdatedUser { get; set; }
    }
}