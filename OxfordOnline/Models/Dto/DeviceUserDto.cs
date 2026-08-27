using OxfordOnline.Models;

namespace OxfordOnline.Models.Dto
{
    public class DeviceUserDto
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? DeviceName { get; set; }
        public string? CustomDeviceName { get; set; }
        public string Platform { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public InventoryHeader? Header { get; set; }
    }
}
