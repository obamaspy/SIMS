using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using SIMS.Validations;

namespace SIMS.Models
{
    public class CategoryViewModel
    {
        public List<CategoryDetail>? CategoryList { get; set; }
    }

    public class CategoryDetail
    {
        //khai bao chi tiet tung ctlg
        public int Id { get; set; }
        [Required(ErrorMessage = "Name category not empty")]
        public string NameCategory { get; set; }
        [AllowNull]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Choose Avatar, please")]
        [AllowedSizeFile(2*1024*1024)]
        [AllowTypeFile(new string[] {".png", ".jpg", ".jpeg", ".gif", ".webp"})]
        public IFormFile? ViewAvatar { get; set; }
        public string? Avatar { get; set; }
        public string? Status { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
