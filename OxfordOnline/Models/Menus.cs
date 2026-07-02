using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("menus")]
    public class Menu
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        [Column("route_name")]
        public string RouteName { get; set; } = string.Empty;

        [Column("image_path")]
        public string? ImagePath { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
