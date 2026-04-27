using System.ComponentModel.DataAnnotations;

namespace net_backend.SubCategories.Contracts;

public record UpdateSubCategoryRequest(
    [Required, StringLength(50, MinimumLength = 1)]
    string Title,

    [StringLength(1000)]
    string? Description,

    [Required, Range(1, int.MaxValue)]
    int CategoryId);
