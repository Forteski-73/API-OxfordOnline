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
    }
}