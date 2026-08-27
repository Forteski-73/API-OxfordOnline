using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OxfordOnline.Models
{
    [Table("inventory_guid")]
    public class InventoryGuid
    {
        [Key]
        [Required]
        [Column("invent_guid")]
        [MaxLength(36)] // Tamanho padrão para GUID/UUID
        public string InventGuid { get; set; } = string.Empty;

        [Required]
        [Column("invent_exp_seq")]
        public int InventExpSeq { get; set; }

        [Column("invent_created")]
        public DateTime? InventCreated { get; set; }

        // Corresponde a `invent_header_id` INT(11) NOT NULL DEFAULT 1, Chave Estrangeira para inventory_header
        [Required]
        [Column("invent_header_id")]
        public int InventHeaderId { get; set; } = 1;

        [ForeignKey(nameof(InventHeaderId))]
        public InventoryHeader? InventoryHeaderNavigation { get; set; }
    }
}