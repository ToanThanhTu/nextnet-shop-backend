using net_backend.Products.Contracts;
using net_backend.Products.Domain;

namespace net_backend.Products.Application.Queries;

public class ListProductsHandler(IProductRepository repo)
{
    public async Task<ProductListPageDto> ExecuteAsync(
        ProductListQuery query, CancellationToken cancellationToken = default)
    {
        var (items, total) = await repo.ListAsync(query, cancellationToken);
        return new ProductListPageDto(items, total);
    }
}
