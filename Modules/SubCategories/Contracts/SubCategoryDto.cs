using net_backend.Data.Types;

namespace net_backend.SubCategories.Contracts;

public record SubCategoryDto(
    int Id,
    string Title,
    string? Slug,
    string? Description,
    int CategoryId)
{
    public static SubCategoryDto FromEntity(SubCategory sc) =>
        new(sc.Id, sc.Title, sc.Slug, sc.Description, sc.CategoryId);
}
