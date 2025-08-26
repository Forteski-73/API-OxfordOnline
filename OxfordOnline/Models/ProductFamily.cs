using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("product_family")]
    public class ProductFamily
    {
        [Key]
        [Column("family_id")]
        [StringLength(4)]
        public string FamilyId { get; set; }

        [Column("family_description")]
        [StringLength(50)]
        public string FamilyDescription { get; set; }

        [Column("status")]
        public int Status { get; set; }
    }
}
