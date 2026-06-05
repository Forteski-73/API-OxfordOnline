namespace OxfordOnline.Models
{
    public class ProfileMenuUpdateRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<MenuDto> Menus { get; set; } = new List<MenuDto>();
    }

    public class MenuDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}