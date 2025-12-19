namespace OxfordOnline.Models.Dto
{
    public class PalletStatusUpdateRequest
    {
        public int PalletId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? UpdatedUser { get; set; }
    }
}
