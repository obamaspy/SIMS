using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SIMS.Database
{
    public class Accounts
    {
        [Key]
        public int Id { get; set; }

        [Column("Username", TypeName = "Varchar(100)"), Required]
        public string Username { get; set; }

        [Column("Email", TypeName = "Varchar(150)"), Required]
        public string Email { get; set; }

        [Column("Phone", TypeName = "Varchar(20)"), Required]
        public string Phone { get; set; }

        [Column("Password", TypeName = "Varchar(255)"), Required]
        public string Password { get; set; }

        [Column("Role", TypeName = "Varchar(50)"), Required]
        public string Role { get; set; }

        [AllowNull]
        public DateTime? CreatedAt { get; set; }

        [AllowNull]
        public DateTime? UpdatedAt { get; set; }

        [AllowNull]
        public DateTime? DeletedAt { get; set; }
    }
}
