using Microsoft.EntityFrameworkCore;
using net_backend.Modules.Cart.Domain;
using net_backend.Data.Types;

namespace net_backend.Modules.Cart.Infrastructure;

public class EfCartRepository(AppDbContext db) : ICartRepository
{
    public Task<List<CartItem>> ListByUserAsync(int userId, CancellationToken cancellationToken = default)
        => db.CartItems
            .AsNoTracking()
            .Where(ci => ci.UserId == userId)
            .Include(ci => ci.Product)               // single SQL JOIN, no N+1
            .OrderBy(ci => ci.Id)
            .ToListAsync(cancellationToken);

    public Task<CartItem?> GetByUserAndProductWithProductAsync(
        int userId, int productId, CancellationToken cancellationToken = default)
        => db.CartItems
            .Where(ci => ci.UserId == userId && ci.ProductId == productId)
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<CartItem> AddAsync(CartItem item, CancellationToken cancellationToken = default)
    {
        db.CartItems.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        // Re-load with Product populated (single query). Avoids a separate
        // FirstOrDefault lookup for Product, which would be the old N+1 trap.
        return await db.CartItems
            .Include(ci => ci.Product)
            .FirstAsync(ci => ci.Id == item.Id, cancellationToken);
    }

    public Task UpdateAsync(CartItem item, CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(CartItem item, CancellationToken cancellationToken = default)
    {
        db.CartItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        // ExecuteDeleteAsync is EF Core 7+: a single DELETE statement, no
        // round-trip per row. Drops to SQL: DELETE FROM cartitems WHERE user_id = $1.
        await db.CartItems
            .Where(ci => ci.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<List<CartItem>> ReplaceForUserAsync(
        int userId,
        IEnumerable<(int ProductId, int Quantity)> items,
        CancellationToken cancellationToken = default)
    {
        // Atomic replace: clear + insert in one transaction.
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        await db.CartItems
            .Where(ci => ci.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        var newItems = items.Select(i => new CartItem
        {
            UserId = userId,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
        }).ToList();

        if (newItems.Count > 0)
        {
            db.CartItems.AddRange(newItems);
            await db.SaveChangesAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);

        // Reload with Product included (single query) and return.
        return await db.CartItems
            .AsNoTracking()
            .Where(ci => ci.UserId == userId)
            .Include(ci => ci.Product)
            .OrderBy(ci => ci.Id)
            .ToListAsync(cancellationToken);
    }
}
