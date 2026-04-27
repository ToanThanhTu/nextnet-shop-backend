using net_backend.Data.Types;

namespace net_backend.Modules.Products.Contracts;

/// <summary>
/// Product detail page response: the product plus its subcategory and
/// parent category so the breadcrumb can be rendered without extra calls.
/// </summary>
public record ProductWithHierarchyDto(
    ProductDto Product,
    HierarchySegment SubCategory,
    HierarchySegment Category)
{
    public static ProductWithHierarchyDto FromEntity(Product p)
    {
        var subCategory = p.SubCategory
            ?? throw new InvalidOperationException(
                $"Product {p.Id} has no SubCategory loaded; ensure repository Includes it.");
        var category = subCategory.Category
            ?? throw new InvalidOperationException(
                $"SubCategory {subCategory.Id} has no Category loaded; ensure repository Includes it.");

        return new ProductWithHierarchyDto(
            ProductDto.FromEntity(p),
            new HierarchySegment(subCategory.Title, subCategory.Slug),
            new HierarchySegment(category.Title, category.Slug));
    }
}

public record HierarchySegment(string Title, string? Slug);
