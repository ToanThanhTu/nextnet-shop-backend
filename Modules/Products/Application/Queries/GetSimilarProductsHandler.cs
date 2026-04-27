using net_backend.Modules.Products.Contracts;
using net_backend.Modules.Products.Domain;

namespace net_backend.Modules.Products.Application.Queries;

public class GetSimilarProductsHandler(IProductRepository repo)
{
    public Task<List<ProductDto>> ExecuteAsync(int productId, CancellationToken cancellationToken = default)
        => repo.ListSimilarAsync(productId, cancellationToken);
}
