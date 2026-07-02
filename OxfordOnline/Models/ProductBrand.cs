using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{

    [Table("product_brand")]
    public class ProductBrand
    {
        [Key]
        [Column("brand_id")]
        [StringLength(4)]
        public string BrandId { get; set; }

        [Column("brand_description")]
        [StringLength(50)]
        public string BrandDescription { get; set; }

        [Column("status")]
        public int Status { get; set; }
    }
    
}
