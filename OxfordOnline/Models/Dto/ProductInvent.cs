namespace OxfordOnline.Models.Dto
{
    public class ProductInvent
    {
        public decimal NetWeight { get; set; }
        public decimal TaraWeight { get; set; }
        public decimal GrossWeight { get; set; }
        public decimal GrossDepth { get; set; }
        public decimal GrossWidth { get; set; }
        public decimal GrossHeight { get; set; }
        public decimal UnitVolume { get; set; }
        public decimal UnitVolumeML { get; set; }
        public int NrOfItems { get; set; }
        public string UnitId { get; set; } = string.Empty;
    }
}
