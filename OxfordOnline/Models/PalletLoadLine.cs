using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("pallet_load_line")]
    public class PalletLoadLine
    {
        [Column("load_id")]
        public int LoadId { get; set; }

        [Column("pallet_id")]
        public int PalletId { get; set; }

        [Required]
        [Column("carregado")]
        public bool Carregado { get; set; } = false;

        [Column("received")]
        public bool Received { get; set; } = false;

        public virtual Pallet Pallet { get; set; }
        
        // Opcional: navegação para relacionamentos
        // public virtual PalletLoadHead LoadHead { get; set; }
        // 
    }
}
