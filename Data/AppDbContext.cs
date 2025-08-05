using FaizHesaplamaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FaizHesaplamaAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Models.Information> Information { get; set; }
        public DbSet<Models.Process> Process { get; set; }
        public DbSet<Models.Users> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite Primary key tanımı:
           // modelBuilder.Entity<Process>().HasKey(p => new { p.ilkTarih, p.faizOrani });


            base.OnModelCreating(modelBuilder);
        }
    }
}
