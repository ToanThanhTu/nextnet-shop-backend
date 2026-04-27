using net_backend.Products.Contracts;
using net_backend.Products.Domain;

namespace net_backend.Products.Application.Queries;

public class GetTopDealsHandler(IProductRepository repo)
{
    public Task<List<ProductDto>> ExecuteAsync(CancellationToken cancellationToken = default)
        => repo.ListTopDealsAsync(limit: 4, cancellationToken);
}
