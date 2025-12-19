using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("product_line")]
    public class ProductLine
    {
        [Key]
        [Column("line_id")]
        [StringLength(4)]
        public string LineId { get; set; }

        [Column("line_description")]
        [StringLength(50)]
        public string LineDescription { get; set; }

        [Column("status")]
        public int Status { get; set; }
    }
}
