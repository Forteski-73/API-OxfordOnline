using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OxfordOnline.Models
{
    [Table("product_pack_image")]
    public class ProductPackImage
    {
        [Column("pack_id")]
        public int PackId { get; set; }

        [Column("pack_sequence")]
        public int PackSequence { get; set; }

        [Required]
        [Column("pack_image_path")]
        [StringLength(500)]
        public string PackImagePath { get; set; }

        [Required]
        [Column("pack_user")]
        [StringLength(100)]
        public string PackUser { get; set; }

        [Column("pack_last_update")]
        public DateTime PackLastUpdate { get; set; }

        // Navigation
        [ForeignKey("PackId")]
        [JsonIgnore]
        public ProductPack? ProductPack { get; set; }
    }
}