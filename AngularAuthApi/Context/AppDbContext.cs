using AngularAuthApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace AngularAuthApi.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options) 
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Cart> Carts{ get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products  { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Cart>().ToTable("CartTable");
            modelBuilder.Entity<Category>().ToTable("CategoryTable");
            modelBuilder.Entity<Order>().ToTable("OrderTable");
            modelBuilder.Entity<Product>().ToTable("ProductTable");
        }

    }
}
