namespace OxfordOnline.Models.Dto
{
    public class ProductApp
    {
        public string ProductId { get; set; } = string.Empty;
        public string? Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ImageZipBase64 { get; set; } = string.Empty;
    }
}
