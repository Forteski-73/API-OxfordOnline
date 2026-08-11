using System.Text.Json.Serialization;

namespace OxfordOnline.Models.Dto
{
    public class SeniorLoginEnvelope
    {
        [JsonPropertyName("jsonToken")]
        public string JsonToken { get; set; } = string.Empty;
    }
}