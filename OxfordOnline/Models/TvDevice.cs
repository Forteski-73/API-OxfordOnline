using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("tv_device")]
    public class TvDevice
    {
        [Key]
        [Column("device_id")]
        public int DeviceId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("setor")]
        public string Setor { get; set; } = string.Empty;

        [MaxLength(60)]
        [Column("trans_code")]
        public string? TransCode { get; set; }

        // Navegação para a tabela pai "device"
        [ForeignKey(nameof(DeviceId))]
        public Device? Device { get; set; }
    }
}