using net_backend.Categories.Domain;
using net_backend.Common.Exceptions;
using net_backend.SubCategories.Contracts;
using net_backend.SubCategories.Domain;

namespace net_backend.SubCategories.Application.Commands;

public class UpdateSubCategoryHandler(
    ISubCategoryRepository subCategoryRepo,
    ICategoryRepository categoryRepo)
{
    public async Task ExecuteAsync(
        int id,
        UpdateSubCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var subCategory = await subCategoryRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"SubCategory {id} not found.", "SUBCATEGORY_NOT_FOUND");

        // Validate the new parent exists (no-op if unchanged, but still cheap).
        var parent = await categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(
                $"Parent Category {request.CategoryId} not found.", "CATEGORY_NOT_FOUND");

        subCategory.Title = request.Title;
        subCategory.Description = request.Description;
        subCategory.CategoryId = parent.Id;

        await subCategoryRepo.UpdateAsync(subCategory, cancellationToken);
    }
}
