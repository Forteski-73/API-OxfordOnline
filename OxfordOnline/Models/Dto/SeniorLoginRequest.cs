using System.Text.Json.Serialization;

namespace OxfordOnline.Models.Dto
{
    public class SeniorLoginRequest
    {
        [JsonPropertyName("accessKey")]
        public string AccessKey { get; set; } = string.Empty;

        [JsonPropertyName("secret")]
        public string Secret { get; set; } = string.Empty;

        [JsonPropertyName("tenantName")]
        public string TenantName { get; set; } = string.Empty;
    }
}