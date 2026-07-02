using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace OxfordOnline.Models
{
    [Table("product")]
    public class Product
    {
        [Key]
        [Column("product_id")]
        [Required]
        [MaxLength(10)]
        public string ProductId { get; set; } = string.Empty;

        [Column("product_name")]
        [MaxLength(255)]
        public string? ProductName { get; set; }

        [Column("barcode")]
        [MaxLength(20)]
        public string? Barcode { get; set; }

        [Column("status")]
        [Required]
        public bool Status { get; set; } = true;

        [Column("note")]
        [MaxLength(255)]
        public string? Note { get; set; }
    }
}