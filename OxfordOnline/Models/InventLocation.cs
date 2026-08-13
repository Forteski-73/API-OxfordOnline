using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("invent_location")]
    public class InventLocation
    {
        [Key]
        [Column("invent_location_id")]
        [MaxLength(10)]
        public string InventLocationId { get; set; } = string.Empty;

        [Required]
        [Column("invent_location_name")]
        [MaxLength(120)]
        public string InventLocationName { get; set; } = string.Empty;

        [Required]
        [Column("status")]
        public bool Status { get; set; } = true;

        #region Propriedades de Navegação

        public virtual ICollection<InventSum> InventSums { get; set; } = new List<InventSum>();

        #endregion
    }
}