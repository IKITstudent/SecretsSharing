using Microsoft.EntityFrameworkCore;
using FileHosting.Models;

namespace FileHosting.Data
{
    public class DBContext:DbContext
    {
        public DBContext(DbContextOptions<DBContext> options):base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Text> Texts { get; set; }
    }
}
