using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("oxf_image")]
    public class Image
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Item))] // Boa prática: usa nameof() ao invés de string
        public int ItemId { get; set; }

        [Required]
        [StringLength(255)]
        public string Path { get; set; } = string.Empty;

        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

        // Relação com o Item - omitida na hora de criar a imagem
        public virtual Item? Item { get; set; } // Mantido para navegação, mas não precisa preencher
    }
}
