using System.Linq.Expressions;
using net_backend.Data.Types;

namespace net_backend.Modules.Products.Contracts;

/// <summary>
/// Wire shape for a product. Fix for audit S4 (8x duplicated DTO projections):
/// the single Projection expression below is reused by every query handler,
/// so SQL only fetches the columns we actually serialise.
/// </summary>
public record ProductDto(
    int Id,
    string Title,
    string? Slug,
    string? Description,
    decimal Price,
    int Sale,
    decimal SalePrice,
    int Stock,
    int? Sold,
    int SubCategoryId)
{
    /// <summary>
    /// Apply this in any IQueryable&lt;Product&gt; pipeline as
    /// <c>.Select(ProductDto.Projection)</c> to translate to SQL projection.
    /// EF Core only generates a SELECT for these columns and avoids loading
    /// the byte[] image bytes.
    /// </summary>
    public static Expression<Func<Product, ProductDto>> Projection =>
        p => new ProductDto(
            p.Id,
            p.Title,
            p.Slug,
            p.Description,
            p.Price,
            p.Sale,
            p.SalePrice,
            p.Stock,
            p.Sold,
            p.SubCategoryId);

    /// <summary>In-memory variant for after-the-fact mapping (e.g. when
    /// you've already loaded the entity).</summary>
    public static ProductDto FromEntity(Product p) =>
        new(p.Id, p.Title, p.Slug, p.Description, p.Price, p.Sale, p.SalePrice, p.Stock, p.Sold, p.SubCategoryId);
}
