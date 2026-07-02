namespace OxfordOnline.Models.Dto
{
    public class FtpImageUploadRequest
    {
        public List<FtpImageUploadItem> Images { get; set; } = new();
    }

    public class FtpImageUploadItem
    {
        public string Url { get; set; } = string.Empty;
        public string Base64Content { get; set; } = string.Empty;
    }

    public class FtpImageUploadResponse
    {
        public string Url { get; set; } = string.Empty;
        public string Status { get; set; } = "Success";
        public string? Message { get; set; }
    }
}