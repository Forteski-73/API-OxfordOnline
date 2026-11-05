using OxfordOnline.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OxfordOnline.Models
{
    [Table("breakage_record")]
    public class BreakageRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("breakage_id")]
        public uint BreakageId { get; set; }

        [Required]
        [Column("product_id")]
        [MaxLength(10)]
        public string ProductId { get; set; } = string.Empty;

        [Column("quantity")]
        public uint Quantity { get; set; } = 1;

        [Required]
        [Column("reason")]
        [MaxLength(200)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [Column("photo_url")]
        [MaxLength(500)]
        public string PhotoUrl { get; set; } = string.Empty;

        // Coluna ENUM do banco: a conversão para texto está em AppDbContext.OnModelCreating.
        [Column("status")]
        public BreakageStatus Status { get; set; } = BreakageStatus.PENDENTE;

        [Column("rejection_reason")]
        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        [Required]
        [Column("created_by")]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("reviewed_by")]
        [MaxLength(100)]
        public string? ReviewedBy { get; set; }

        [Column("reviewed_at")]
        public DateTime? ReviewedAt { get; set; }

        [Column("written_off_by")]
        [MaxLength(100)]
        public string? WrittenOffBy { get; set; }

        [Column("written_off_at")]
        public DateTime? WrittenOffAt { get; set; }

        [ForeignKey(nameof(ProductId))]
        [JsonIgnore]
        public Product? ProductNavigation { get; set; }
    }
}
