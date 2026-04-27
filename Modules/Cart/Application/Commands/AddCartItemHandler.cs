using net_backend.Modules.Cart.Contracts;
using net_backend.Modules.Cart.Domain;
using net_backend.Data.Types;

namespace net_backend.Modules.Cart.Application.Commands;

/// <summary>
/// Add to cart: if the user already has this product, increment the
/// quantity; otherwise insert a new row. Returns the updated/created
/// item with its Product populated.
/// </summary>
public class AddCartItemHandler(ICartRepository repo)
{
    public async Task<CartItemDto> ExecuteAsync(
        int userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await repo.GetByUserAndProductWithProductAsync(
            userId, request.ProductId, cancellationToken);

        if (existing is not null)
        {
            existing.Quantity += request.Quantity;
            await repo.UpdateAsync(existing, cancellationToken);
            return CartItemDto.FromEntity(existing);
        }

        var saved = await repo.AddAsync(new CartItem
        {
            UserId = userId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
        }, cancellationToken);

        return CartItemDto.FromEntity(saved);
    }
}
