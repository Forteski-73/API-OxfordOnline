namespace OxfordOnline.Models.Dto
{
    public class InventoryRecordResponse
    {
        public string InventGuid { get; set; } = string.Empty;
        public string InventCode { get; set; } = string.Empty;
        public decimal InventTotal { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
