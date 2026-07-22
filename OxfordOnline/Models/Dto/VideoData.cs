namespace OxfordOnline.Models
{
    public class VideoData
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string VideoUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public uint? DurationSeconds { get; set; }
        public uint VideoOrder { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryColor { get; set; }
        public string? CategoryIcon { get; set; }
    }
}