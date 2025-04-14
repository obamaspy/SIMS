using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SIMS.Database
{
    public class Courses
    {
        [Key]
        public int Id { get; set; }

        [Column("CoursesName", TypeName = "Varchar(100)"), Required]
        public string? CoursesName { get; set; }

        [Column("Subject", TypeName = "Varchar(150)"), Required]
        public string? Subject { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
