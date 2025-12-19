namespace OxfordOnline.Models.Dto
{
    public class InventoryRecordRequest
    {
        public string InventGuid { get; set; } = string.Empty;
        public string InventCode { get; set; } = string.Empty;
        public List<InventoryRecord> Records { get; set; } = new();
    }
}
