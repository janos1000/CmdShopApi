using Microsoft.EntityFrameworkCore;

namespace CmdShopApi.Models
{
    public class ComputerShopDbContext : DbContext
    {
        public ComputerShopDbContext(DbContextOptions options) : base(options)
        {
        }

        public ComputerShopDbContext()
        {
        }

        public DbSet<Osystem> Osystems { get; set; }
        public DbSet<Computer> Computers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=localhost;database=computer;user=root;password=");
        }
    }
}