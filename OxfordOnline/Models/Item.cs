using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OxfordOnline.Models
{
    [Table("oxf_item")] // Nome da tabela no banco
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("item")] // Mapeia para a coluna correta no banco
        public string CodeItem { get; set; } = string.Empty;

        [Required]
        [Column("qr_code")]
        public string QrCode { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("create_date")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
