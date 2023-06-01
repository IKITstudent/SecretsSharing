using Microsoft.EntityFrameworkCore;
using FileHosting.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FileHosting.Data
{
    public class DBContext: IdentityDbContext<User>
    {
        public DbSet<Text> Texts { get; set; }
        //public DbSet<User> Users { get; set; }
        public DBContext(DbContextOptions<DBContext> options):base(options)
        {
            Database.EnsureCreated();
        }

    }
}
