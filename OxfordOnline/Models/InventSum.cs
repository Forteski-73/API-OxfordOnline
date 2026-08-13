using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OxfordOnline.Models
{
    [Table("invent_sum")]
    public class InventSum
    {
        [Key]
        [Column("product_id", Order = 0)]
        [MaxLength(10)]
        public string ProductId { get; set; } = string.Empty;

        [Key]
        [Column("invent_location_id", Order = 1)]
        [MaxLength(10)]
        public string InventLocationId { get; set; } = string.Empty;

        [Required]
        [Column("avail_physical", TypeName = "decimal(12,2)")]
        public decimal AvailPhysical { get; set; } = 0.00m;

        [Column("updated_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [JsonIgnore]
        public DateTime? UpdatedAt { get; set; }

        /*
        #region Propriedades de Navegação (Chaves Estrangeiras)

        [ForeignKey(nameof(ProductId))]
        public virtual Product? Product { get; set; }

        [ForeignKey(nameof(InventLocationId))]
        public virtual InventLocation? InventLocation { get; set; }

        #endregion
        */
    }
}