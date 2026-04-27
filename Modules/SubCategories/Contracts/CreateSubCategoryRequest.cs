using System.ComponentModel.DataAnnotations;

namespace net_backend.Modules.SubCategories.Contracts;

public record CreateSubCategoryRequest(
    [Required, StringLength(50, MinimumLength = 1)]
    string Title,

    [StringLength(1000)]
    string? Description,

    [Required, Range(1, int.MaxValue)]
    int CategoryId);
