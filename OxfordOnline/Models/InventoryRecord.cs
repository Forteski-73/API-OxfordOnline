using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OxfordOnline.Models
{
    [Table("inventory_record")]
    public class InventoryRecord
    {
        // Corresponde a `id` INT(11) NOT NULL AUTO_INCREMENT, PRIMARY KEY
        [Key]
        [Column("id")]
        public int? Id { get; set; }

        // Corresponde a `invent_code` VARCHAR(50) NOT NULL (FK para inventory)
        [Required]
        [Column("invent_code")]
        [MaxLength(50)]
        public string InventCode { get; set; } = string.Empty;

        // Corresponde a `invent_created` DATETIME NULL DEFAULT NULL
        [Column("invent_created")]
        public DateTime? InventCreated { get; set; }

        // Corresponde a `invent_user` VARCHAR(100) NULL DEFAULT NULL
        [Column("invent_user")]
        [MaxLength(100)]
        public string? InventUser { get; set; }

        // Corresponde a `invent_unitizer` VARCHAR(25) NULL DEFAULT NULL
        [Column("invent_unitizer")]
        [MaxLength(25)]
        public string? InventUnitizer { get; set; }

        // Corresponde a `invent_location` VARCHAR(25) NULL DEFAULT NULL
        [Column("invent_location")]
        [MaxLength(25)]
        public string? InventLocation { get; set; }

        // Corresponde a `invent_product` VARCHAR(10) NOT NULL (FK para product.product_id)
        [Required]
        [Column("invent_product")]
        [MaxLength(10)]
        public string InventProduct { get; set; } = string.Empty;

        // Corresponde a `invent_barcode` VARCHAR(20) NULL DEFAULT NULL
        [Column("invent_barcode")]
        [MaxLength(20)]
        public string? InventBarcode { get; set; }

        // Corresponde a `invent_standard_stack` INT(11) NULL DEFAULT NULL
        [Column("invent_standard_stack")]
        public int? InventStandardStack { get; set; }

        // Corresponde a `invent_qtd_stack` INT(11) NULL DEFAULT NULL
        [Column("invent_qtd_stack")]
        public int? InventQtdStack { get; set; }

        // Corresponde a `invent_qtd_individual` INT(11) NULL DEFAULT NULL
        [Column("invent_qtd_individual")]
        public decimal? InventQtdIndividual { get; set; }

        // Corresponde a `invent_total` INT(11) NULL DEFAULT NULL
        // Nota: O valor total é frequentemente calculado (InventQtdStack * InventStandardStack) + InventQtdIndividual
        [Column("invent_total")]
        public decimal? InventTotal { get; set; }


        // --- PROPRIEDADES DE NAVEGAÇÃO (CHAVES ESTRANGEIRAS) ---

        [ForeignKey(nameof(InventCode))]
        [JsonIgnore]
        public Inventory? InventoryNavigation { get; set; }

        [ForeignKey(nameof(InventProduct))]
        [JsonIgnore]
        public Product? ProductNavigation { get; set; }
    }
}
