using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("pallet_item")]
    public class PalletItem
    {
        [Column("pallet_id")]
        public int PalletId { get; set; }

        [Column("product_id")]
        public string ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("status")]
        public string Status { get; set; }
    }
}