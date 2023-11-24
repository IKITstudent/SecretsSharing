using Microsoft.EntityFrameworkCore;
using FileHosting.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FileHosting.Data
{
    public class DBContext : IdentityDbContext<User>
    {
        public DbSet<File> Files { get; set; }
        public override DbSet<User> Users { get; set; }
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

    }
}
