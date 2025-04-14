using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SIMS.Databases
{
    public class Categories
    {
        [Key]
        public int Id { get; set; }

        [Column("NameCategory", TypeName = "Varchar(60)"), Required]

        public string NameCategory { get; set; }

        [Column("Description", TypeName = "Varchar(150)"), AllowNull]

        public string? Description { get; set; }

        [Column("Avatar", TypeName = "Varchar(150)"), AllowNull]

        public string? Avatar { get; set; }

        [Column("Status", TypeName = "Varchar(20)"), Required]
        public string Status { get; set; }

        [AllowNull]
        public DateTime? CreatedAt { get; set; }

        [AllowNull]
        public DateTime? UpdatedAt { get; set; }

        [AllowNull]
        public DateTime? DeletedAt { get; set; }






    }
}