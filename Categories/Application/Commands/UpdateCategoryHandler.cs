using net_backend.Categories.Domain;
using net_backend.Categories.Contracts;
using net_backend.Common.Exceptions;

namespace net_backend.Categories.Application.Commands;

/// <summary>
/// Update an existing category's mutable fields. Throws NotFoundException
/// if the id doesn't resolve.
/// </summary>
public class UpdateCategoryHandler(ICategoryRepository repo)
{
    public async Task ExecuteAsync(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await repo.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Category {id} not found.", "CATEGORY_NOT_FOUND");

        // Mutate the tracked entity; SaveChanges in repo.UpdateAsync persists.
        category.Title = request.Title;
        category.Description = request.Description;

        await repo.UpdateAsync(category, cancellationToken);
    }
}
