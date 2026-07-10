using System.ComponentModel.DataAnnotations;

namespace OxfordOnline.Models.Dto
{
    public class TvDeviceRequest
    {
        [Required]
        public string Setor { get; set; } = string.Empty;

        public string? TransCode { get; set; }
    }
}