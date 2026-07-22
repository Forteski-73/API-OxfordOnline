using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("video_categories")]
    public class VideoCategory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("slug")]
        public string Slug { get; set; } = string.Empty;

        [Column("color_hex")]
        public string ColorHex { get; set; } = "#F59E0B";

        [Column("icon_name")]
        public string? IconName { get; set; }

        [Column("display_order")]
        public uint DisplayOrder { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public ICollection<Video>? Videos { get; set; }
    }
}