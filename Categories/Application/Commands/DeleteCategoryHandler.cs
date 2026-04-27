using net_backend.Categories.Domain;
using net_backend.Common.Exceptions;

namespace net_backend.Categories.Application.Commands;

/// <summary>
/// Delete a category by id. Throws NotFoundException if absent so the
/// caller gets 404 instead of a silent no-op.
/// </summary>
public class DeleteCategoryHandler(ICategoryRepository repo)
{
    public async Task ExecuteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await repo.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Category {id} not found.", "CATEGORY_NOT_FOUND");

        await repo.DeleteAsync(category, cancellationToken);
    }
}
