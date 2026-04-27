using net_backend.Categories.Contracts;
using net_backend.Categories.Domain;

namespace net_backend.Categories.Application.Queries;

/// <summary>
/// Lists all categories with their subcategories. Read-only; no side effects.
/// </summary>
public class ListCategoriesHandler(ICategoryRepository repo)
{
    public async Task<List<CategoryDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var categories = await repo.ListWithSubCategoriesAsync(cancellationToken);
        return categories.Select(CategoryDto.FromEntity).ToList();
    }
}
