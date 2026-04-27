using net_backend.Data.Types;
using net_backend.Modules.Products.Contracts;

namespace net_backend.Modules.Products.Domain;

/// <summary>
/// Persistence contract for the Product aggregate.
///
/// Read methods return <see cref="ProductDto"/> directly so EF Core can
/// project at the SQL level — only the columns we serialise are fetched,
/// keeping the byte[] Image off the wire for list endpoints.
///
/// Mutation paths take and return the entity (Product) since update and
/// delete need a tracked instance.
/// </summary>
public interface IProductRepository
{
    Task<(List<ProductDto> items, int totalItems)> ListAsync(
        ProductListQuery query, CancellationToken cancellationToken = default);

    Task<(List<ProductDto> items, int totalItems)> ListOnSaleAsync(
        ProductListQuery query, CancellationToken cancellationToken = default);

    Task<(List<ProductDto> items, int totalItems)> ListBestsellersAsync(
        ProductListQuery query, CancellationToken cancellationToken = default);

    Task<List<ProductDto>> ListTopDealsAsync(int limit, CancellationToken cancellationToken = default);

    Task<List<ProductDto>> SearchByTitleAsync(string query, CancellationToken cancellationToken = default);

    Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Find by slug, including SubCategory + parent Category.
    /// Returns the entity so the caller can build the hierarchy DTO.</summary>
    Task<Product?> GetBySlugWithHierarchyAsync(string slug, CancellationToken cancellationToken = default);

    Task<List<ProductDto>> ListSimilarAsync(int productId, CancellationToken cancellationToken = default);

    Task<List<ProductDto>> ListByUserPurchaseHistoryAsync(int userId, CancellationToken cancellationToken = default);

    Task<byte[]?> GetImageAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Load the entity (not DTO) for mutation paths.</summary>
    Task<Product?> GetEntityByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Product product, CancellationToken cancellationToken = default);
}
