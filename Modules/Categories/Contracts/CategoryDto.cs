using net_backend.Data.Types;

namespace net_backend.Categories.Contracts;

/// <summary>
/// Wire-format Category. Don't return entities directly from controllers;
/// always project to this DTO so internal shape changes don't leak.
/// </summary>
public record CategoryDto(
    int Id,
    string Title,
    string? Slug,
    string? Description,
    List<SubCategorySummaryDto> SubCategories)
{
    public static CategoryDto FromEntity(Category category) => new(
        category.Id,
        category.Title,
        category.Slug,
        category.Description,
        category.SubCategories?
            .Select(SubCategorySummaryDto.FromEntity)
            .ToList() ?? []);
}

/// <summary>
/// Trimmed-down subcategory representation for nesting under a category.
/// Distinct from a full SubCategoryDto (which would belong to the
/// SubCategories feature).
/// </summary>
public record SubCategorySummaryDto(int Id, string Title, string? Slug, string? Description)
{
    public static SubCategorySummaryDto FromEntity(SubCategory sc) =>
        new(sc.Id, sc.Title, sc.Slug, sc.Description);
}
