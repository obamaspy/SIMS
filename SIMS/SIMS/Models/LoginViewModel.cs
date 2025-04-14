using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email cannot be empty")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password cannot be empty")]
        public string Password { get; set; }

        //public string Role { get; set; }
    }
}
