using net_backend.Data.Types;

namespace net_backend.Modules.Orders.Contracts;

public record OrderItemDto(
    int Id,
    int ProductId,
    int Quantity,
    decimal Price,
    OrderProductSummaryDto? Product)
{
    public static OrderItemDto FromEntity(OrderItem item) => new(
        item.Id,
        item.ProductId,
        item.Quantity,
        item.Price,
        item.Product is null ? null : OrderProductSummaryDto.FromEntity(item.Product));
}

/// <summary>
/// Slim product summary embedded in order items (no byte[] image).
/// Distinct from CartProductSummaryDto so the two DTOs can diverge if
/// each context grows its own needs.
/// </summary>
public record OrderProductSummaryDto(
    int Id,
    string Title,
    string? Slug)
{
    public static OrderProductSummaryDto FromEntity(Product product) =>
        new(product.Id, product.Title, product.Slug);
}
