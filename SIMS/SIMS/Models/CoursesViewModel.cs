using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS.Models
{
    public class CoursesViewModel
    {
        public List<CoursesDetail> CoursesList { get; set; } = new List<CoursesDetail>();
    }
    public class CoursesDetail
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name Courses not empty")]
        public string? CoursesName { get; set; }

        [Required(ErrorMessage = "Subject not empty")]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "Start date not empty")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "End date not empty")]
        public DateTime? EndDate { get; set; }

        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
