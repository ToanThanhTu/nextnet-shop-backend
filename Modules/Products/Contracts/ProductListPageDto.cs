namespace net_backend.Modules.Products.Contracts;

/// <summary>
/// Paginated list response. Frontend reads { products, totalItems }
/// to drive its pagination component.
/// </summary>
public record ProductListPageDto(
    List<ProductDto> Products,
    int TotalItems);
