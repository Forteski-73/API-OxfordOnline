using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OxfordOnline.Models
{
    [Table("pallet_group_image")]
    public class PalletGroupImage
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
        public PalletGroup? ProductPack { get; set; }
    }
}