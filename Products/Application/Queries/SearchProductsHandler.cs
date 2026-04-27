using net_backend.Products.Contracts;
using net_backend.Products.Domain;

namespace net_backend.Products.Application.Queries;

public class SearchProductsHandler(IProductRepository repo)
{
    public Task<List<ProductDto>> ExecuteAsync(string query, CancellationToken cancellationToken = default)
        => repo.SearchByTitleAsync(query, cancellationToken);
}
