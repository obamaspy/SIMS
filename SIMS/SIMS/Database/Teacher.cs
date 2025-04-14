using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace SIMS.Database
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [Column("TeacherName", TypeName = "Varchar(100)"), Required]
        public string TeacherName { get; set; }

        [Column("Email", TypeName = "Varchar(150)"), Required]
        public string? Email { get; set; }

        [Column("Address", TypeName = "Varchar(255)"), AllowNull]
        public string? Address { get; set; }

        [Column("Subject", TypeName = "Varchar(255)"), AllowNull]
        public string? Subject { get; set; }

        [Column("Avatar", TypeName = "Varchar(150)"), AllowNull]
        public string? Avatar { get; set; }

        [AllowNull]
        public DateTime? CreatedAt { get; set; }

        [AllowNull]
        public DateTime? UpdatedAt { get; set; }

        [AllowNull]
        public DateTime? DeletedAt { get; set; }
    }
}
