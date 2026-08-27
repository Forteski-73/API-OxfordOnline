using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("inventory_header")]
    public class InventoryHeader
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("invent_name")]
        [MaxLength(50)]
        public string InventName { get; set; } = string.Empty;

        [Column("invent_description")]
        [MaxLength(255)]
        public string? InventDescription { get; set; }

        [Column("sales_channel_only")]
        public bool SalesChannelOnly { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;
    }
}
