using OxfordOnline.Models.Dto;

namespace OxfordOnline.Models
{
    public class LoginUserRequest
    {
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int? ProfileId { get; set; }
        public DeviceInfo? Device { get; set; }
    }
}
