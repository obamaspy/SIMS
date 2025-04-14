using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS.Validations;

namespace SIMS.Models
{
    public class StudentViewModel
    {
        public List<StudentDetail> StudentList { get; set; } = new List<StudentDetail>();
    }

    public class StudentDetail
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name cannot be empty")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Student Code cannot be empty")]
        public string? StudentCode { get; set; }

        [Required(ErrorMessage = "Email cannot be empty")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Address cannot be empty")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Course cannot be empty")]
        public string Course { get; set; }

        [AllowedSizeFile(2 * 1024 * 1024)]
        [AllowTypeFile(new string[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" })]
        public IFormFile? ViewAvatar { get; set; }
        public string? Avatar { get; set; }

        public List<SelectListItem> CourseList { get; set; } = new(); // Initialize to avoid null

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
