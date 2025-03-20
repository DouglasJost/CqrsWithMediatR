using CqrsWithMediatR.Models;
using Microsoft.EntityFrameworkCore;

namespace CqrsWithMediatR.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options) { }

        public required DbSet<Product> Products { get; set; }
        public required DbSet<ProductReadOnly> ProductsReadOnly { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) 
            {
                return;
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                // Explicitly set table name
                entity.ToTable("Products");

                // Define primary key
                entity.HasKey(p => p.Id);     

                entity.Property(p => p.Name)
                    .HasColumnType("varchar")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(p => p.Price)
                    .HasColumnType("DECIMAL(10,2)");

                // Enforce RowVersion for concurrency checks
                entity.Property(p => p.RowVersion)
                    .IsRowVersion();  
            });

            modelBuilder.Entity<ProductReadOnly>(entity => 
            {
                // Explicitly set table name
                entity.ToTable("ProductsReadOnly");

                // Define primary key
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .HasColumnType("varchar")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(p => p.Price)
                    .HasColumnType("DECIMAL(10,2)");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
