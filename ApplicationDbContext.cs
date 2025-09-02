using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProducts> OrderProducts { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Category> Categories => Set<Category>(); // DbSet for Category entity
        public DbSet<Supplier> Suppliers => Set<Supplier>();  // DbSet for Supplier entity

        protected override void OnModelCreating(ModelBuilder modelBuilder) // Override the OnModelCreating method to configure the model
        {
            modelBuilder.Entity<User>() // Configure the User entity
                        .HasIndex(u => u.Email) // Create an index on the Email property
                        .IsUnique(); // Ensure the index is unique

            modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
