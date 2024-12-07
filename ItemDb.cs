using Microsoft.EntityFrameworkCore;

class ItemDb : DbContext
{
  public ItemDb(DbContextOptions<ItemDb> options)
    : base(options) { }

  public DbSet<Item> Items => Set<Item>();
}
