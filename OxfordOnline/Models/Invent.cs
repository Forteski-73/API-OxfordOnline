using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OxfordOnline.Models
{
    [Table("product_invent")]
    public class Invent
    {
        [Key]
        [Column("id")]
        [JsonIgnore]
        public int Id { get; set; }

        [Required]
        [Column("product_id")]
        [MaxLength(50)]
        public string ProductId { get; set; } = string.Empty;

        [Column("net_weight")]
        public decimal? NetWeight { get; set; }

        [Column("tara_weight")]
        public decimal? TaraWeight { get; set; }

        [Column("gross_weight")]
        public decimal? GrossWeight { get; set; }

        [Column("gross_depth")]
        public decimal? GrossDepth { get; set; }

        [Column("gross_width")]
        public decimal? GrossWidth { get; set; }

        [Column("gross_height")]
        public decimal? GrossHeight { get; set; }

        [Column("unit_volume")]
        public decimal? UnitVolume { get; set; }

        [Column("unit_volume_ml")]
        public decimal? UnitVolumeML { get; set; }

        [Column("nr_of_items")]
        public int? NrOfItems { get; set; }

        [Column("unit_id")]
        [MaxLength(20)]
        public string? UnitId { get; set; }

        // Opcional: relacionamento com a tabela Product
        //[ForeignKey("ProductId")]
        //public Product? Product { get; set; }
    }
}
