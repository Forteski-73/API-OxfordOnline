using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("videos")]
    public class Video
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("video_url")]
        public string VideoUrl { get; set; } = string.Empty;

        [Column("thumbnail_url")]
        public string? ThumbnailUrl { get; set; }

        [Column("duration_seconds")]
        public uint? DurationSeconds { get; set; }

        [Column("display_order")]
        public uint DisplayOrder { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("view_count")]
        public uint ViewCount { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public VideoCategory? Category { get; set; }
    }
}