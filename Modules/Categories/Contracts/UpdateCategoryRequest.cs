using System.ComponentModel.DataAnnotations;

namespace net_backend.Modules.Categories.Contracts;

/// <summary>
/// Input DTO for PUT /categories/{id}. Same shape as Create today; kept as a
/// separate type so the two can diverge (e.g. partial update via PATCH later).
/// </summary>
public record UpdateCategoryRequest(
    [Required, StringLength(50, MinimumLength = 1)]
    string Title,

    [StringLength(1000)]
    string? Description);
