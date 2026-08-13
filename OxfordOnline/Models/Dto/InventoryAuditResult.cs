using System.Text.Json.Serialization;

namespace OxfordOnline.Models.Dtos
{
    public class InventoryAuditResult
    {

        [JsonPropertyName("inventLocationId")]
        public string InventLocationId { get; set; } = string.Empty;

        [JsonPropertyName("inventProduct")]
        public string InventProduct { get; set; } = string.Empty;

        [JsonPropertyName("inventBarcode")]
        public string? InventBarcode { get; set; }

        [JsonPropertyName("totalInvent")]
        public decimal TotalInvent { get; set; }

        [JsonPropertyName("availPhysical")]
        public decimal AvailPhysical { get; set; }

    }
}