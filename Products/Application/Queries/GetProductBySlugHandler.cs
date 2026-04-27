using net_backend.Common.Exceptions;
using net_backend.Products.Contracts;
using net_backend.Products.Domain;

namespace net_backend.Products.Application.Queries;

public class GetProductBySlugHandler(IProductRepository repo)
{
    public async Task<ProductWithHierarchyDto> ExecuteAsync(string slug, CancellationToken cancellationToken = default)
    {
        var product = await repo.GetBySlugWithHierarchyAsync(slug, cancellationToken)
            ?? throw new NotFoundException($"Product '{slug}' not found.", "PRODUCT_NOT_FOUND");
        return ProductWithHierarchyDto.FromEntity(product);
    }
}
