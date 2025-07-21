﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("product_oxford")]
    public class Oxford
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("product_id")]
        [MaxLength(50)]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [Column("family_id")]
        [MaxLength(50)]
        public string FamilyId { get; set; } = string.Empty;

        [Column("family_description")]
        [MaxLength(255)]
        public string? FamilyDescription { get; set; }

        [Required]
        [Column("brand_id")]
        [MaxLength(50)]
        public string BrandId { get; set; } = string.Empty;

        [Column("brand_description")]
        [MaxLength(255)]
        public string? BrandDescription { get; set; }

        [Required]
        [Column("decoration_id")]
        [MaxLength(50)]
        public string DecorationId { get; set; } = string.Empty;

        [Column("decoration_description")]
        [MaxLength(255)]
        public string? DecorationDescription { get; set; }

        [Required]
        [Column("type_id")]
        [MaxLength(50)]
        public string TypeId { get; set; } = string.Empty;

        [Column("type_description")]
        [MaxLength(255)]
        public string? TypeDescription { get; set; }

        [Required]
        [Column("process_id")]
        [MaxLength(50)]
        public string ProcessId { get; set; } = string.Empty;

        [Column("process_description")]
        [MaxLength(255)]
        public string? ProcessDescription { get; set; }

        [Required]
        [Column("situation_id")]
        [MaxLength(50)]
        public string SituationId { get; set; } = string.Empty;

        [Column("situation_description")]
        [MaxLength(255)]
        public string? SituationDescription { get; set; }

        [Required]
        [Column("line_id")]
        [MaxLength(50)]
        public string LineId { get; set; } = string.Empty;

        [Column("line_description")]
        [MaxLength(255)]
        public string? LineDescription { get; set; }

        [Required]
        [Column("quality_id")]
        [MaxLength(50)]
        public string QualityId { get; set; } = string.Empty;

        [Column("quality_description")]
        [MaxLength(255)]
        public string? QualityDescription { get; set; }

        [Required]
        [Column("base_product_id")]
        [MaxLength(50)]
        public string BaseProductId { get; set; } = string.Empty;

        [Column("base_product_description")]
        [MaxLength(255)]
        public string? BaseProductDescription { get; set; }

        [Required]
        [Column("product_group_id")]
        [MaxLength(50)]
        public string ProductGroupId { get; set; } = string.Empty;

        [Column("product_group_description")]
        [MaxLength(255)]
        public string? ProductGroupDescription { get; set; }

        // Relacionamento com Product
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}
