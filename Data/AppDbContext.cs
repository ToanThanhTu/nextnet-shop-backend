using Microsoft.EntityFrameworkCore;
using net_backend.Data.Types;

class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Category> Categories => Set<Category>();
  public DbSet<SubCategory> SubCategories => Set<SubCategory>();
  public DbSet<Product> Products => Set<Product>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder
      .Entity<Category>()
      .HasMany(c => c.SubCategories)
      .WithOne(sc => sc.Category)
      .HasForeignKey(sc => sc.CategoryId);

    modelBuilder
      .Entity<SubCategory>()
      .HasMany(sc => sc.Products)
      .WithOne(p => p.SubCategory)
      .HasForeignKey(p => p.SubCategoryId);
  }
}
