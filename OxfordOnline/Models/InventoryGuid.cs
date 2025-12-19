using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OxfordOnline.Models
{
    [Table("inventory_guid")]
    public class InventoryGuid
    {
        // Corresponde a `invent_guid` VARCHAR(36) NOT NULL, AGORA PRIMARY KEY
        [Key] // Define esta propriedade como a Chave Primária
        [Required]
        [Column("invent_guid")]
        [MaxLength(36)] // Tamanho padrão para GUID/UUID
        public string InventGuid { get; set; } = string.Empty;

        // Corresponde a `invent_exp_seq` INT(10) NOT NULL
        [Required]
        [Column("invent_exp_seq")]
        public int InventExpSeq { get; set; }

        // Corresponde a `invent_created` DATETIME NULL DEFAULT NULL
        [Column("invent_created")]
        public DateTime? InventCreated { get; set; }
    }
}