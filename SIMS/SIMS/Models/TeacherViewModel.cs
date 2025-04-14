using System.ComponentModel.DataAnnotations;
using SIMS.Validations;

namespace SIMS.Models
{
    public class TeacherViewModel
    {
        public List<TeacherDetail> TeacherList { get; set; } = new List<TeacherDetail>();
    }

    public class TeacherDetail
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name Teacher not empty")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email Teacher not empty")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Address not empty")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Subject not empty")]
        public string? Subject { get; set; }

        
        [AllowedSizeFile(2 * 1024 * 1024)]
        [AllowTypeFile(new string[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" })]
        public IFormFile? ViewAvatar { get; set; }
        public string? Avatar { get; set; }

        public DateTime? CreateAt { get; set; }

        public DateTime? UpdateAt { get; set; }
    }
}
