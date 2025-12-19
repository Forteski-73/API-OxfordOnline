namespace OxfordOnline.Models.Dto
{
    public class ProductSearchResponse
    {
        public int TotalProducts { get; set; }
        public IEnumerable<ProductApp> Products { get; set; } = new List<ProductApp>();

    }
}
