using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxfordOnline.Models
{
    [Table("user_account")]
    public class UserAccount
    {
        [Key]
        public int Id { get; set; }

        public string? Branch { get; set; }
        public string? Name { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }

        [Column("group")]
        public string? Group { get; set; }  // "group" é palavra reservada, por isso o [Column]

        [Column("work_shift")]
        public string? WorkShift { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = null!;  // NOT NULL e UNIQUE no banco

        public string? Extension { get; set; }
        public string? Phone { get; set; }

        [Column("add_location")]
        public string? AddLocation { get; set; }
    }
}
