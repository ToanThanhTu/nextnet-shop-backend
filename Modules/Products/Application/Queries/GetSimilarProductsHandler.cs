using net_backend.Products.Contracts;
using net_backend.Products.Domain;

namespace net_backend.Products.Application.Queries;

public class GetSimilarProductsHandler(IProductRepository repo)
{
    public Task<List<ProductDto>> ExecuteAsync(int productId, CancellationToken cancellationToken = default)
        => repo.ListSimilarAsync(productId, cancellationToken);
}
