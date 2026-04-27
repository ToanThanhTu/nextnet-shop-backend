using System.ComponentModel.DataAnnotations;

namespace net_backend.Modules.Products.Contracts;

/// <summary>
/// Input shape for POST /products. Excludes computed columns (Slug,
/// SalePrice) and analytics fields (Sold) so the client can't write
/// to them; the database/entity owns those.
/// </summary>
public record CreateProductRequest(
    [Required, StringLength(50, MinimumLength = 1)] string Title,
    [StringLength(1000)] string? Description,
    [Required, Range(0.01, 999999.99)] decimal Price,
    [Required, Range(0, 100)] int Sale,
    [Required, Range(0, int.MaxValue)] int Stock,
    [Required, Range(1, int.MaxValue)] int SubCategoryId);
