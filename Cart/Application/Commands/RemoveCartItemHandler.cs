using net_backend.Cart.Domain;
using net_backend.Common.Exceptions;

namespace net_backend.Cart.Application.Commands;

public class RemoveCartItemHandler(ICartRepository repo)
{
    public async Task ExecuteAsync(int userId, int productId, CancellationToken cancellationToken = default)
    {
        var item = await repo.GetByUserAndProductWithProductAsync(userId, productId, cancellationToken)
            ?? throw new NotFoundException(
                $"Product {productId} is not in your cart.",
                "CART_ITEM_NOT_FOUND");

        await repo.DeleteAsync(item, cancellationToken);
    }
}
