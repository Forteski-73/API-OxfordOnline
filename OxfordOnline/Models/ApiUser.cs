using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("api_user")]
    public class ApiUser
    {
        public int Id { get; set; }
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string Account { get; set; } = string.Empty;
    }
}
