using BarnManagement.Models.Entities;
using BarnManagement.WinForms.Models.Entities;
using System.Data.Entity;

namespace BarnManagement.WinForms.Models
{
    public class BarnContext : DbContext
    {
        public BarnContext() : base("BarnContext") { }

        public DbSet<User> Users { get; set; }
        public DbSet<Barn> Barns { get; set; }
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>()
                .Property(x => x.Username).IsRequired().HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(x => x.Role).IsRequired().HasMaxLength(20);

            modelBuilder.Entity<Animal>()
                .Property(x => x.Type).IsRequired().HasMaxLength(30);

            modelBuilder.Entity<Animal>()
                .Property(x => x.Gender).IsRequired().HasMaxLength(10);

            modelBuilder.Entity<Product>()
                .Property(x => x.ProductType).IsRequired().HasMaxLength(30);

            modelBuilder.Entity<Product>()
                .Property(x => x.Quantity).HasPrecision(18, 3);

            modelBuilder.Entity<Sale>()
                .Property(x => x.UnitPrice).HasPrecision(18, 2);

            modelBuilder.Entity<Sale>()
                .Property(x => x.Quantity).HasPrecision(18, 3);

            base.OnModelCreating(modelBuilder);
        }
    }
}
