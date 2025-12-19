namespace OxfordOnline.Models
{
    public class AppProductFilterRequest
    {
        public List<string>? ProductId { get; set; }
        public string? Name { get; set; }
        public List<string>? BrandId { get; set; }
        public List<string>? LineId { get; set; }
        public List<string>? FamilyId { get; set; }
        public List<string>? DecorationId { get; set; }
        public List<string>? Tag { get; set; }
        public string? YesNoImage { get; set; }
    }
}
