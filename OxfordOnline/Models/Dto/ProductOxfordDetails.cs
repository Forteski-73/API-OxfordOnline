namespace OxfordOnline.Models.Dto
{
    public class ProductOxfordDetails
    {
        public string ProductId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Line { get; set; } = string.Empty;
        public string Decoration { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;

        public Dictionary<string, string> Images { get; set; } = new();

        public ProductInvent Invent { get; set; } = new();
        public List<Tag>? Tags { get; set; }
    }
}
