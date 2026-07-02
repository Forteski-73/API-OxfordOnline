using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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

        [Column("quantity_received")]
        public int QuantityReceived { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("status")]
        public string Status { get; set; } = "I";

        // Propriedade de Navegação para a tabela Product
        // O atributo [JsonIgnore] impede que esta propriedade seja serializada
        // no JSON de retorno da API (ocultando-a do Swagger e dos GETs).
        [ForeignKey("ProductId")]
        [JsonIgnore]
        public Product? Product { get; set; }

        // Produto Nome para retornar no JSON (não mapeado no banco)
        [NotMapped]
        public string? ProductName { get; set; }
    }
}