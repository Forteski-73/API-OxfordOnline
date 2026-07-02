using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OxfordOnline.Models
{
    [Table("pallet_image")]
    public class PalletImage
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("pallet_id")]
        [Required]
        public int PalletId { get; set; }

        [Column("image_path")]
        [Required] 
        [StringLength(500)] 
        public string ImagePath { get; set; }
    }
}