using net_backend.Categories.Contracts;
using net_backend.Categories.Domain;
using net_backend.Common.Exceptions;

namespace net_backend.Categories.Application.Queries;

/// <summary>
/// Fetch a single category by id. Throws NotFoundException if absent so the
/// global exception handler returns a 404 ProblemDetails.
/// </summary>
public class GetCategoryByIdHandler(ICategoryRepository repo)
{
    public async Task<CategoryDto> ExecuteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await repo.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Category {id} not found.", "CATEGORY_NOT_FOUND");
        return CategoryDto.FromEntity(category);
    }
}
