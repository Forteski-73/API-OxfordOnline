using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OxfordOnline.Models
{
    [Table("product_tax_information")]
    public class TaxInformation
    {
        [Key]
        [Column("id")]
        [JsonIgnore]
        public int Id { get; set; }

        [Required]
        [Column("product_id")]
        [MaxLength(50)]
        public string ProductId { get; set; } = string.Empty;

        // Tributação
        [Column("taxation_origin")]
        [MaxLength(255)]
        public string? TaxationOrigin { get; set; }

        [Column("tax_fiscal_classification")]
        [MaxLength(50)]
        public string? TaxFiscalClassification { get; set; }

        [Column("product_type")]
        [MaxLength(50)]
        public string? ProductType { get; set; }

        [Column("cest_code")]
        [MaxLength(50)]
        public string? CestCode { get; set; }

        [Column("fiscal_group_id")]
        [MaxLength(50)]
        public string? FiscalGroupId { get; set; }

        // Percentuais aproximados de imposto
        [Column("approx_tax_value_federal", TypeName = "decimal(10,4)")]
        public decimal? ApproxTaxValueFederal { get; set; }

        [Column("approx_tax_value_state", TypeName = "decimal(10,4)")]
        public decimal? ApproxTaxValueState { get; set; }

        [Column("approx_tax_value_city", TypeName = "decimal(10,4)")]
        public decimal? ApproxTaxValueCity { get; set; }

        // Relacionamento com Product (Opcional)
        //[ForeignKey("ProductId")]
        //public Product? Product { get; set; }
    }
}