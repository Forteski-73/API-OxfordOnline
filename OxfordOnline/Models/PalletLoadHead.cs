// PalletLoadHead.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("pallet_load_head")]
    public class PalletLoadHead
    {
        [Key]
        [Column("load_id")]
        public int LoadId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)] // Tamanho definido para ENUM no SQL
        [Column("status")]
        public string Status { get; set; } = "Carregando";

        [Required]
        [Column("date", TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [Column("time", TypeName = "time")]
        public TimeSpan Time { get; set; }

        [Required]
        [Column("created_user")]
        public string CreatedUser { get; set; }
    }
}
