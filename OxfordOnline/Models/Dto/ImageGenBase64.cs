using OxfordOnline.Models.Enums;

namespace OxfordOnline.Models.Dto
{
    public class ImageGenBase64
    {
        public string CodeId { get; set; } = string.Empty; // Identificador gererico
        public List<string>? Base64Images { get; set; }

        public string CreatedUser { get; set; } = string.Empty;
    }
}