using net_backend.SubCategories.Contracts;
using net_backend.SubCategories.Domain;

namespace net_backend.SubCategories.Application.Queries;

public class ListSubCategoriesHandler(ISubCategoryRepository repo)
{
    public async Task<List<SubCategoryDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var subCategories = await repo.ListAllAsync(cancellationToken);
        return subCategories.Select(SubCategoryDto.FromEntity).ToList();
    }
}
