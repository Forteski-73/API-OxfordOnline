using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("pallet")]
    public class Pallet
    {
        [Key]
        [Column("pallet_id")]
        public int PalletId { get; set; }

        [Column("total_quantity")]
        public int TotalQuantity { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("location")]
        public string Location { get; set; }

        [Column("created_user")]
        public string CreatedUser { get; set; }

        [Column("updated_user")]
        public string UpdatedUser { get; set; }

        [Column("image_path")]
        public string? ImagePath { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}