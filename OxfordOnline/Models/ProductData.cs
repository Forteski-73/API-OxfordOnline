using OxfordOnline.Models;

namespace OxfordOnline.Models
{
    public class ProductData
    {
        public Product? Product { get; set; }
        public Oxford? Oxford { get; set; }
        public Invent? Invent { get; set; }
        public TaxInformation? TaxInformation { get; set; }
        //public List<Image>? Images { get; set; }
    }
}
