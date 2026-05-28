using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("profile_menus")]
    public class ProfileMenu
    {
        [Column("profile_id")]
        public int ProfileId { get; set; }

        [Column("menu_id")]
        public int MenuId { get; set; }
    }
}