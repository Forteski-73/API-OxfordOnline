namespace OxfordOnline.Models.Dto
{
    public class DeviceInfo
    {
        public string Guid { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty; // "ios" | "android" | "web"
        public string? DeviceName { get; set; }
        public string? AppVersion { get; set; }
    }
}
