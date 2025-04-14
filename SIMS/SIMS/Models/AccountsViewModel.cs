using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class AccountsViewModel
    {
        public List<AccountsDetail>? AccountsList { get; set; }
    }
    public class AccountsDetail
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string? Username { get; set; }

        [Required, EmailAddress, StringLength(150)]
        public string? Email { get; set; }

        [Required, Phone, StringLength(20)]
        public string? Phone { get; set; }

        [Required, DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required, StringLength(50)]
        public string? Role { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
