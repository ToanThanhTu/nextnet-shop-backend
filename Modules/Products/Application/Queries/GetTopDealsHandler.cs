using net_backend.Modules.Products.Contracts;
using net_backend.Modules.Products.Domain;

namespace net_backend.Modules.Products.Application.Queries;

public class GetTopDealsHandler(IProductRepository repo)
{
    public Task<List<ProductDto>> ExecuteAsync(CancellationToken cancellationToken = default)
        => repo.ListTopDealsAsync(limit: 4, cancellationToken);
}
