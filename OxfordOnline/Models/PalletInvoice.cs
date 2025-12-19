using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("pallet_invoice")]
    [
        Microsoft.EntityFrameworkCore.PrimaryKey(
            nameof(LoadId),
            nameof(Invoice)
        )
    ]
    public class PalletInvoice
    {
        [Required]
        [Column("load_id")]
        public int LoadId { get; set; }

        [Required]
        [Column("invoice")]
        [StringLength(50)]
        public string Invoice { get; set; } = string.Empty;


        [Required]
        [Column("invoice_key")]
        [StringLength(100)]
        public string InvoiceKey { get; set; } = string.Empty;

        // Opcional: Para representar a relação com a tabela 'pallet_load_head' (se for Entity Framework Core)
        // public PalletLoadHead LoadHead { get; set; }
    }
}
