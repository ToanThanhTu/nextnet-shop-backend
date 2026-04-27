using net_backend.Modules.Orders.Contracts;
using net_backend.Modules.Orders.Domain;

namespace net_backend.Modules.Orders.Application.Queries;

public class GetMyOrdersHandler(IOrderRepository repo)
{
    public async Task<List<OrderDto>> ExecuteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var orders = await repo.ListByUserAsync(userId, cancellationToken);
        return orders.Select(OrderDto.FromEntity).ToList();
    }
}
