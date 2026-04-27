using net_backend.Common.Exceptions;
using net_backend.Modules.SubCategories.Domain;

namespace net_backend.Modules.SubCategories.Application.Commands;

public class DeleteSubCategoryHandler(ISubCategoryRepository repo)
{
    public async Task ExecuteAsync(int id, CancellationToken cancellationToken = default)
    {
        var subCategory = await repo.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"SubCategory {id} not found.", "SUBCATEGORY_NOT_FOUND");

        await repo.DeleteAsync(subCategory, cancellationToken);
    }
}
