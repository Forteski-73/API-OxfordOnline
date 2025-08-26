using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("product_line")]
    public class ProductLine
    {
        [Key]
        [Column("lines_id")]
        [StringLength(4)]
        public string LinesId { get; set; }

        [Column("lines_description")]
        [StringLength(50)]
        public string LinesDescription { get; set; }

        [Column("status")]
        public int Status { get; set; }
    }
}
