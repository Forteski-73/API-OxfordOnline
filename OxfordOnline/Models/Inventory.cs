using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    // Enum para mapear o invent_status
    public enum InventoryStatus
    {
        Iniciado = 1,
        Finalizado = 2
    }

    [Table("inventory")]
    public class Inventory
    {
        // A coluna 'id' foi removida do esquema do DB.
        // O campo 'invent_code' é agora a Chave Primária.

        // Corresponde a `invent_code` VARCHAR(50) NOT NULL, AGORA PRIMARY KEY
        [Key]
        [Required]
        [Column("invent_code")]
        [MaxLength(50)]
        public string InventCode { get; set; } = string.Empty;

        // Corresponde a `invent_guid` VARCHAR(36) NOT NULL, Chave Estrangeira
        [Required]
        [Column("invent_guid")]
        [MaxLength(36)]
        public string? InventGuid { get; set; }

        // Corresponde a `invent_sector` VARCHAR(100) NULL DEFAULT NULL
        [Column("invent_sector")]
        [MaxLength(100)]
        public string? InventSector { get; set; }

        // Corresponde a `invent_created` DATETIME NULL DEFAULT NULL
        [Column("invent_created")]
        public DateTime? InventCreated { get; set; }

        // Corresponde a `invent_user` VARCHAR(100) NULL DEFAULT NULL
        [Column("invent_user")]
        [MaxLength(100)]
        public string? InventUser { get; set; }

        // Corresponde a `invent_status` ENUM('Iniciado','Finalizado') NOT NULL
        [Required]
        [Column("invent_status")]
        [EnumDataType(typeof(InventoryStatus))]
        public string InventStatus { get; set; } = InventoryStatus.Iniciado.ToString(); // Default 'Iniciado'

        // Corresponde a `invent_total` INT(11) NULL DEFAULT NULL
        [Column("invent_total")]
        public decimal InventTotal { get; set; }


        // --- PROPRIEDADES DE NAVEGAÇÃO ---

        // Nova propriedade de navegação: Coleção de Records associados a este cabeçalho.
        // O EF Core irá inferir o uso de InventCode para esta relação.
        public ICollection<InventoryRecord>? Records { get; set; }
    }
}