using net_backend.Modules.Cart.Contracts;
using net_backend.Modules.Cart.Domain;

namespace net_backend.Modules.Cart.Application.Queries;

public class GetMyCartHandler(ICartRepository repo)
{
    public async Task<List<CartItemDto>> ExecuteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var items = await repo.ListByUserAsync(userId, cancellationToken);
        return items.Select(CartItemDto.FromEntity).ToList();
    }
}
