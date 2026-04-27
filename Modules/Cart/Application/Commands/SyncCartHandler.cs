using net_backend.Modules.Cart.Contracts;
using net_backend.Modules.Cart.Domain;

namespace net_backend.Modules.Cart.Application.Commands;

/// <summary>
/// Replace the user's server-side cart with the items the client supplies.
/// Used at sign-in time to merge a guest-session cart into the persisted one.
/// Atomic: all rows are deleted and inserted in a single transaction (see
/// ICartRepository.ReplaceForUserAsync).
/// </summary>
public class SyncCartHandler(ICartRepository repo)
{
    public async Task<List<CartItemDto>> ExecuteAsync(
        int userId,
        SyncCartRequest request,
        CancellationToken cancellationToken = default)
    {
        var pairs = request.Items.Select(i => (i.ProductId, i.Quantity));
        var items = await repo.ReplaceForUserAsync(userId, pairs, cancellationToken);
        return items.Select(CartItemDto.FromEntity).ToList();
    }
}
