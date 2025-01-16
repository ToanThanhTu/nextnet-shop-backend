using Microsoft.EntityFrameworkCore;
using net_backend.Data.Types;

class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

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

        modelBuilder
            .Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId);

        modelBuilder
            .Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId);

        modelBuilder
            .Entity<CartItem>()
            .HasOne(ci => ci.User)
            .WithMany(u => u.CartItems)
            .HasForeignKey(ci => ci.UserId);
    }
}
