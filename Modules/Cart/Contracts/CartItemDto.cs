using net_backend.Data.Types;

namespace net_backend.Modules.Cart.Contracts;

/// <summary>
/// Wire shape for a cart item. Embeds a slim product summary so the cart
/// UI can render without a second fetch, but doesn't ship the full Product
/// entity (which carries byte[] image bytes and the User back-reference).
/// </summary>
public record CartItemDto(
    int Id,
    int ProductId,
    int Quantity,
    CartProductSummaryDto Product)
{
    public static CartItemDto FromEntity(CartItem item) => new(
        item.Id,
        item.ProductId,
        item.Quantity,
        CartProductSummaryDto.FromEntity(item.Product
            ?? throw new InvalidOperationException(
                $"CartItem {item.Id} has no Product loaded; ensure repository Includes it.")));
}

/// <summary>
/// Just the product fields the cart UI needs. Image bytes are excluded;
/// the frontend hits /products/{id}/image for the bytes.
/// </summary>
public record CartProductSummaryDto(
    int Id,
    string Title,
    string? Slug,
    decimal Price,
    int Sale,
    decimal SalePrice,
    int Stock)
{
    public static CartProductSummaryDto FromEntity(Product product) => new(
        product.Id,
        product.Title,
        product.Slug,
        product.Price,
        product.Sale,
        product.SalePrice,
        product.Stock);
}
