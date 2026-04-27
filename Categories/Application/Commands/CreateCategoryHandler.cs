using net_backend.Categories.Contracts;
using net_backend.Categories.Domain;
using net_backend.Data.Types;

namespace net_backend.Categories.Application.Commands;

/// <summary>
/// Create a new category from a validated request. Returns the persisted
/// category as a DTO so the controller can include the assigned id.
/// </summary>
public class CreateCategoryHandler(ICategoryRepository repo)
{
    public async Task<CategoryDto> ExecuteAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            Title = request.Title,
            Description = request.Description,
        };

        var saved = await repo.AddAsync(category, cancellationToken);
        return CategoryDto.FromEntity(saved);
    }
}
