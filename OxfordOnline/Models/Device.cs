using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("device")]
    public class Device
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("guid")]
        [MaxLength(36)]
        public Guid Guid { get; set; } = Guid.Empty;

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("device_name")]
        [MaxLength(100)]
        public string? DeviceName { get; set; }

        [Column("custom_device_name")]
        [MaxLength(100)]
        public string? CustomDeviceName { get; set; }

        [Required]
        [Column("platform")]
        public string Platform { get; set; } = string.Empty;

        [Column("app_version")]
        [MaxLength(20)]
        public string? AppVersion { get; set; }

        [Column("first_login_at")]
        public DateTime FirstLoginAt { get; set; }

        [Column("last_seen_at")]
        public DateTime LastSeenAt { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        // Navegação: usuário dono do device
        [ForeignKey(nameof(UserId))]
        public ApiUser? User { get; set; }

        // Navegação: relação 1:1 com tv_device
        //public TvDevice? TvDevice { get; set; }
    }
}