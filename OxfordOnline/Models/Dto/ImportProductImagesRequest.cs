using OxfordOnline.Models.Enums;

namespace OxfordOnline.Models.Dto
{
    public class ImportProductImagesRequest
    {
        public Finalidade Finalidade { get; set; }
        public List<ProductImageUrl> Images { get; set; } = new();
    }
}
