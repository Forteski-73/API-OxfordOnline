namespace OxfordOnline.Models.Dto
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Registration { get; set; } = string.Empty;
        public string? Position { get; set; } = string.Empty;
        public string? CompanyName { get; set; } = string.Empty;
        public string? Department { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? WorkShift { get; set; } = string.Empty;
        public string? PhoneContact { get; set; } = string.Empty;
        
    }
}