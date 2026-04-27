using net_backend.Modules.Cart.Domain;

namespace net_backend.Modules.Cart.Application.Commands;

public class ClearCartHandler(ICartRepository repo)
{
    public Task ExecuteAsync(int userId, CancellationToken cancellationToken = default)
        => repo.ClearByUserAsync(userId, cancellationToken);
}
