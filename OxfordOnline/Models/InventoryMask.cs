using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OxfordOnline.Models.Enums;

namespace OxfordOnline.Models
{
    [Table("inventory_mask")]
    public class InventoryMask
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Armazena o valor do Enum como String no banco de dados (MySQL ENUM)
        /// </summary>
        [Required]
        [Column("field_name")]
        public string FieldName { get; set; } = string.Empty;

        [Required]
        [Column("field_mask")]
        [MaxLength(255)]
        public string FieldMask { get; set; } = string.Empty;

        /// <summary>
        /// Propriedade para facilitar o uso do Enum Mask no C#
        /// </summary>
        [NotMapped]
        public Mask FieldType
        {
            get => Enum.TryParse(FieldName, out Mask result) ? result : Mask.Codigo;
            set => FieldName = value.ToString();
        }
    }
}