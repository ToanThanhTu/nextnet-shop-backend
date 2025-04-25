using Microsoft.EntityFrameworkCore;
using net_backend.Data.Types;

class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options ?? throw new ArgumentNullException(nameof(options)))
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
        // Users
        modelBuilder.Entity<User>()
            .ToTable("users")
            .HasKey(u => u.Id);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Orders)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.CartItems)
            .WithOne(ci => ci.User)
            .HasForeignKey(ci => ci.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Categories
        modelBuilder.Entity<Category>()
            .ToTable("categories")
            .HasKey(c => c.Id);

        modelBuilder.Entity<Category>()
            .HasMany(c => c.SubCategories)
            .WithOne(sc => sc.Category)
            .HasForeignKey(sc => sc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // SubCategories
        modelBuilder.Entity<SubCategory>()
            .ToTable("subcategories")
            .HasKey(sc => sc.Id);

        modelBuilder.Entity<SubCategory>()
            .HasMany(sc => sc.Products)
            .WithOne(p => p.SubCategory)
            .HasForeignKey(p => p.SubCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Products
        modelBuilder.Entity<Product>()
            .ToTable("products")
            .HasKey(p => p.Id);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.SubCategory)
            .WithMany(sc => sc.Products)
            .HasForeignKey(p => p.SubCategoryId);

        // Orders
        modelBuilder.Entity<Order>()
            .ToTable("orders")
            .HasKey(o => o.Id);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderItems
        modelBuilder.Entity<OrderItem>()
            .ToTable("orderitems")
            .HasKey(oi => oi.Id);

        // CartItems
        modelBuilder.Entity<CartItem>()
            .ToTable("cartitems")
            .HasKey(ci => ci.Id);

        // Indexes
        modelBuilder.Entity<Product>().HasIndex(p => p.SubCategoryId);
        modelBuilder.Entity<SubCategory>().HasIndex(sc => sc.CategoryId);
        modelBuilder.Entity<Order>().HasIndex(o => o.UserId);
        modelBuilder.Entity<OrderItem>().HasIndex(oi => oi.OrderId);
        modelBuilder.Entity<CartItem>().HasIndex(ci => ci.UserId);
    }
}
