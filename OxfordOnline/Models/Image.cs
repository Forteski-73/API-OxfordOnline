using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OxfordOnline.Models
{
    [Table("product_image")]
    public class Image
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        [JsonIgnore]
        public int Id { get; set; }

        [Required]
        [Column("product_id")]
        [StringLength(10)]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [Column("image_path")]
        [StringLength(500)]
        public string ImagePath { get; set; } = string.Empty;

        [Column("sequence")]
        public int Sequence { get; set; } = 1;

        [Column("image_main")]
        public bool ImageMain { get; set; } = false;

        [Required]
        [Column("finalidade")]
        public string Finalidade { get; set; } = "PRODUTO";

        // Navegação
        [ForeignKey(nameof(ProductId))]
        [JsonIgnore]
        public virtual Product? Product { get; set; }
    }
}