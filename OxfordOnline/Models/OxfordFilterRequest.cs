using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OxfordOnline.Models
{
    public class OxfordFilterRequest
    {
        public List<string>? ProductId { get; set; }
        public List<string>? FamilyId { get; set; }
        public List<string>? BrandId { get; set; }
        public List<string>? DecorationId { get; set; }
        public List<string>? TypeId { get; set; }
        public List<string>? ProcessId { get; set; }
        public List<string>? SituationId { get; set; }
        public List<string>? LineId { get; set; }
        public List<string>? QualityId { get; set; }
        public List<string>? BaseProductId { get; set; }
        public List<string>? ProductGroupId { get; set; }
    }
}