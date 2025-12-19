using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("product_attribute_map")]
    public class ProductAttributeMap
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("brand_id")]
        [StringLength(4)]
        public string BrandId { get; set; }

        [Column("line_id")]
        [StringLength(4)]
        public string LineId { get; set; }

        [Column("decoration_id")]
        [StringLength(6)]
        public string DecorationId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Propriedades de navegação para EF Core
        [ForeignKey("BrandId")]
        public ProductBrand? Brand { get; set; }

        [ForeignKey("LineId")]
        public ProductLine? Line { get; set; }

        [ForeignKey("DecorationId")]
        public ProductDecoration? Decoration { get; set; }
    }
}