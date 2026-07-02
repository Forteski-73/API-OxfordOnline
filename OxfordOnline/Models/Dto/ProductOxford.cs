namespace OxfordOnline.Models.Dto
{
    public class ProductOxford
    {
        public string ProductId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Line { get; set; } = string.Empty;
        public string Decoration { get; set; } = string.Empty;
        public string ThumbUrl { get; set; } = string.Empty;
        public ProductInvent Invent { get; set; } = new();
    }
}
