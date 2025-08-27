using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("product_decoration")]
    public class ProductDecoration
    {
        [Key]
        [Column("decoration_id")]
        [StringLength(6)]
        public string DecorationId { get; set; }

        [Column("decoration_description")]
        [StringLength(100)]
        public string DecorationDescription { get; set; }

        [Column("status")]
        public int Status { get; set; }
    }
}