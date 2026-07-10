using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OxfordOnline.Models
{
    [Table("pallet_group_item")]
    public class PalletGroupItem
    {
        [Key, Column("pack_id", Order = 0)]
        public int PackId { get; set; }

        [Key]
        [Required]
        [Column("pack_product_id")]
        [MaxLength(10)]
        public string PackProductId { get; set; } = string.Empty;

        [Required]
        [Column("pack_user")]
        [StringLength(100)]
        public string PackUser { get; set; } = string.Empty;

        [NotMapped]
        public string? ProductName => Product?.ProductName;

        // Navigation Properties

        [JsonIgnore]
        [ForeignKey("PackId")]
        public virtual PalletGroup? ProductPack { get; set; }

        [JsonIgnore]
        [ForeignKey("PackProductId")]
        public virtual Product? Product { get; set; }

    }
}