using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OxfordOnline.Models
{
    [Table("pallet_group")]
    public class PalletGroup
    {
        [Key]
        [Column("pack_id")]
        public int PackId { get; set; }

        [Required]
        [Column("pack_name")]
        [StringLength(50)]
        public string PackName { get; set; }

        [Required]
        [Column("pack_user")]
        [StringLength(100)]
        public string PackUser { get; set; }

        [Column("pack_created")]
        public DateTime PackCreated { get; set; }

        // Navigation Properties 
        // Inicia com uma lista vazia para evitar erros de validação 'Required'

        public ICollection<PalletGroupImage> Images { get; set; } = new List<PalletGroupImage>();


        public ICollection<PalletGroupItem> Items { get; set; } = new List<PalletGroupItem>();
    }
}