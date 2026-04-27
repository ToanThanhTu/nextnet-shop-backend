using net_backend.Categories.Domain;
using net_backend.Common.Exceptions;
using net_backend.Data.Types;
using net_backend.SubCategories.Contracts;
using net_backend.SubCategories.Domain;

namespace net_backend.SubCategories.Application.Commands;

/// <summary>
/// Cross-aggregate validation: a SubCategory cannot exist without its parent
/// Category. We check via ICategoryRepository (the *interface*, not the EF
/// implementation) so this handler stays decoupled from EF Core.
/// </summary>
public class CreateSubCategoryHandler(
    ISubCategoryRepository subCategoryRepo,
    ICategoryRepository categoryRepo)
{
    public async Task<SubCategoryDto> ExecuteAsync(
        CreateSubCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var parent = await categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(
                $"Parent Category {request.CategoryId} not found.", "CATEGORY_NOT_FOUND");

        var subCategory = new SubCategory
        {
            Title = request.Title,
            Description = request.Description,
            CategoryId = parent.Id,
        };

        var saved = await subCategoryRepo.AddAsync(subCategory, cancellationToken);
        return SubCategoryDto.FromEntity(saved);
    }
}
