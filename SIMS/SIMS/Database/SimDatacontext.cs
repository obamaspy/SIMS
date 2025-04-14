using Microsoft.EntityFrameworkCore;
using SIMS.Database;
using SIMS.Databases;

namespace SIMS.Database
{
    public class SimDatacontext : DbContext
    {
        public SimDatacontext(DbContextOptions<SimDatacontext> options) : base(options)
        {

        }

        public DbSet<Categories> Categories { get; set; }
        public DbSet<Courses> Courses { get; set; }
        public DbSet<Accounts> Accounts { get; set; }
        public DbSet<Teacher> Teacher { get; set; }

        public DbSet<Student> Student { get; set; }

    }
}
