using net_backend.Data.Types;

namespace net_backend.Orders.Domain;

public interface IOrderRepository
{
    /// <summary>List a user's orders, eagerly loading items + product summaries.</summary>
    Task<List<Order>> ListByUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Find a single order by id; returns null if not found.</summary>
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Persist changes to a tracked order.</summary>
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
}
