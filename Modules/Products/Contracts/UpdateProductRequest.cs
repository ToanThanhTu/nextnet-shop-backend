using System.ComponentModel.DataAnnotations;

namespace net_backend.Modules.Products.Contracts;

public record UpdateProductRequest(
    [Required, StringLength(50, MinimumLength = 1)] string Title,
    [StringLength(1000)] string? Description,
    [Required, Range(0.01, 999999.99)] decimal Price,
    [Required, Range(0, 100)] int Sale,
    [Required, Range(0, int.MaxValue)] int Stock);
