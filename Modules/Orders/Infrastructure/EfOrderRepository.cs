using Microsoft.EntityFrameworkCore;
using net_backend.Data.Types;
using net_backend.Modules.Orders.Domain;

namespace net_backend.Modules.Orders.Infrastructure;

public class EfOrderRepository(AppDbContext db) : IOrderRepository
{
    public Task<List<Order>> ListByUserAsync(int userId, CancellationToken cancellationToken = default)
        => db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems!)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => db.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);
}
