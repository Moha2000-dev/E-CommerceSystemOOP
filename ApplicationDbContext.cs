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
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public object OrderItems { get; internal set; }

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

            modelBuilder.Entity<OrderProducts>()
                .HasOne(op => op.product)
                .WithMany(p => p.OrderProducts)   // add collection to Product if you want
                .HasForeignKey(op => op.PID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .Property(o => o.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<Product>()
                .Property(p => p.RowVersion)
                .IsRowVersion();

            // In DbContext.OnModelCreating
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(x => x.UID);
                e.Property(x => x.UID).ValueGeneratedOnAdd();        // identity
                e.Property(x => x.Email).IsRequired().HasMaxLength(256);
                e.Property(x => x.Password).IsRequired().HasMaxLength(200);
                e.Property(x => x.UName).IsRequired().HasMaxLength(100);
                e.Property(x => x.Role).HasConversion<int>().IsRequired();
                e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                // Optional, if you want uniqueness (recommended)
                e.HasIndex(x => x.Email).IsUnique();
            });


            modelBuilder.Entity<User>(e =>
            {
                e.Property(x => x.Password).IsRequired().HasMaxLength(100);
                e.Property(x => x.Email).IsRequired().HasMaxLength(256);
                e.Property(x => x.Role).HasConversion<int>().IsRequired();
            });

        }
    }
}
