using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace OxfordOnline.Models
{
    [Table("product")]
    public class Product
    {
        [Key]
        [Column("item_id")]
        [MaxLength(20)]
        public string ItemId { get; set; } = string.Empty;

        [Column("item_barcode")]
        public string? ItemBarCode { get; set; }

        [Column("prod_brand_id")]
        public string? ProdBrandId { get; set; }

        [Column("prod_brand_description_id")]
        public string? ProdBrandDescriptionId { get; set; }

        [Column("prod_lines_id")]
        public string? ProdLinesId { get; set; }

        [Column("prod_lines_description_id")]
        public string? ProdLinesDescriptionId { get; set; }

        [Column("prod_decoration_id")]
        public string? ProdDecorationId { get; set; }

        [Column("prod_decoration_description_id")]
        public string? ProdDecorationDescriptionId { get; set; }

        [Column("name_item")]
        public string? Name { get; set; }

        [Column("unit_volume_ml")]
        [Precision(10, 2)]
        public decimal? UnitVolumeML { get; set; }

        [Column("item_net_weight")]
        [Precision(10, 2)]
        public decimal? ItemNetWeight { get; set; }

        [Column("prod_family_id")]
        public string? ProdFamilyId { get; set; }

        [Column("prod_family_description_id")]
        public string? ProdFamilyDescriptionId { get; set; }

        [Column("gross_weight")]
        [Precision(10, 2)]
        public decimal? GrossWeight { get; set; }

        [Column("tara_weight")]
        [Precision(10, 2)]
        public decimal? TaraWeight { get; set; }

        [Column("gross_depth")]
        [Precision(10, 2)]
        public decimal? GrossDepth { get; set; }

        [Column("gross_width")]
        [Precision(10, 2)]
        public decimal? GrossWidth { get; set; }

        [Column("gross_height")]
        [Precision(10, 2)]
        public decimal? GrossHeight { get; set; }

        [Column("nr_of_items")]
        [Precision(10, 2)]
        public decimal? NrOfItems { get; set; }

        [Column("tax_fiscal_Classification")]
        public string? TaxFiscalClassification { get; set; }
    }
}
