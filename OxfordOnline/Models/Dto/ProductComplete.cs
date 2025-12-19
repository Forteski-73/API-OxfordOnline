namespace OxfordOnline.Models.Dto
{
    public class ProductComplete
    {
        public Product? Product { get; set; }
        public Oxford? Oxford { get; set; }
        public Invent? Invent { get; set; }
        public InventDim? Location { get; set; }
        public TaxInformation? TaxInformation { get; set; }
        public List<ImageBase64>? Images { get; set; }
        public List<Tag>? Tags { get; set; }
    }
}
