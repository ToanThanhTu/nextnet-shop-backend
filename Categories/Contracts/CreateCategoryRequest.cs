using System.ComponentModel.DataAnnotations;

namespace net_backend.Categories.Contracts;

/// <summary>
/// Input DTO for POST /categories. Field annotations drive ASP.NET Core's
/// automatic model validation; on bind failure, [ApiController] returns
/// a 400 ProblemDetails before the controller method runs.
/// </summary>
public record CreateCategoryRequest(
    [Required, StringLength(50, MinimumLength = 1)]
    string Title,

    [StringLength(1000)]
    string? Description);
