using net_backend.Cart.Domain;

namespace net_backend.Cart.Application.Commands;

public class ClearCartHandler(ICartRepository repo)
{
    public Task ExecuteAsync(int userId, CancellationToken cancellationToken = default)
        => repo.ClearByUserAsync(userId, cancellationToken);
}
