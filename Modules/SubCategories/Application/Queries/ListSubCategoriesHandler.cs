using net_backend.Modules.SubCategories.Contracts;
using net_backend.Modules.SubCategories.Domain;

namespace net_backend.Modules.SubCategories.Application.Queries;

public class ListSubCategoriesHandler(ISubCategoryRepository repo)
{
    public async Task<List<SubCategoryDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var subCategories = await repo.ListAllAsync(cancellationToken);
        return subCategories.Select(SubCategoryDto.FromEntity).ToList();
    }
}
