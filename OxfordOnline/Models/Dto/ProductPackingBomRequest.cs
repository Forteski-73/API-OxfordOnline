namespace OxfordOnline.Models.Dto
{
    public class ProductPackingBomRequest
    {
        public string ProductId { get; set; } = string.Empty;
        public List<BomItem> BomItems { get; set; } = new List<BomItem>();
    }

    public class BomItem
    {
        public string? ProductBomId { get; set; }
        public string? ProductName { get; set; }
        public int ProductQty { get; set; }
        public int ProductSeq { get; set; }
        public string? UpdatedUser { get; set; }
    }
}