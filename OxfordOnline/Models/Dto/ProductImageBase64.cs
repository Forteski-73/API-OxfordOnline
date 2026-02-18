using OxfordOnline.Models.Enums;

namespace OxfordOnline.Models.Dto
{
    public class ProductImageBase64
    {
        public string ProductId { get; set; }
        public Finalidade Finalidade { get; set; }
        public List<string>? Base64Images { get; set; }
    }
}
