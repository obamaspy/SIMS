using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SIMS.Database
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Column("StudentName", TypeName = "Varchar(100)"), Required]
        public string StudentName { get; set; }

        [Column("StudentCode", TypeName = "Varchar(50)")]
        public string? StudentCode { get; set; }

        [Column("Email", TypeName = "Varchar(150)"), Required]
        public string Email { get; set; }

        [Column("Address", TypeName = "Varchar(255)")]
        public string? Address { get; set; }

        [Column("Course", TypeName = "Varchar(150)"), Required]
        public string Course { get; set; }

        [Column("Avatar", TypeName = "Varchar(150)")] // Added Avatar field
        public string? Avatar { get; set; } // Nullable Avatar field to allow no avatar

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
