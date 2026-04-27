using net_backend.Modules.Products.Contracts;
using net_backend.Modules.Products.Domain;

namespace net_backend.Modules.Products.Application.Queries;

public class SearchProductsHandler(IProductRepository repo)
{
    public Task<List<ProductDto>> ExecuteAsync(string query, CancellationToken cancellationToken = default)
        => repo.SearchByTitleAsync(query, cancellationToken);
}
