using net_backend.Common.Exceptions;
using net_backend.Modules.SubCategories.Contracts;
using net_backend.Modules.SubCategories.Domain;

namespace net_backend.Modules.SubCategories.Application.Queries;

public class GetSubCategoryByIdHandler(ISubCategoryRepository repo)
{
    public async Task<SubCategoryDto> ExecuteAsync(int id, CancellationToken cancellationToken = default)
    {
        var subCategory = await repo.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"SubCategory {id} not found.", "SUBCATEGORY_NOT_FOUND");
        return SubCategoryDto.FromEntity(subCategory);
    }
}
