using net_backend.Modules.Cart.Contracts;
using net_backend.Modules.Cart.Domain;
using net_backend.Common.Exceptions;

namespace net_backend.Modules.Cart.Application.Commands;

/// <summary>
/// Set the quantity of an existing cart item to an absolute value.
/// Throws NotFoundException if the (user, product) row doesn't exist —
/// callers should use AddCartItem to create it.
/// </summary>
public class UpdateCartItemHandler(ICartRepository repo)
{
    public async Task<CartItemDto> ExecuteAsync(
        int userId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await repo.GetByUserAndProductWithProductAsync(
            userId, request.ProductId, cancellationToken)
            ?? throw new NotFoundException(
                $"Product {request.ProductId} is not in your cart.",
                "CART_ITEM_NOT_FOUND");

        item.Quantity = request.Quantity;
        await repo.UpdateAsync(item, cancellationToken);
        return CartItemDto.FromEntity(item);
    }
}
