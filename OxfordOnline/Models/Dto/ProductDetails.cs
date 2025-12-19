namespace OxfordOnline.Models.Dto
{
    public class ProductDetails
    {
        public Product?         Product { get; set; }
        public Oxford?          Oxford { get; set; }
        public TaxInformation?  TaxInformation { get; set; }
        public Invent?          Invent { get; set; }
        public InventDim?       Location { get; set; }
        public List<Image>?     Images { get; set; }
    }
}
