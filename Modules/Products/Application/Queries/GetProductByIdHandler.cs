using net_backend.Common.Exceptions;
using net_backend.Modules.Products.Contracts;
using net_backend.Modules.Products.Domain;

namespace net_backend.Modules.Products.Application.Queries;

public class GetProductByIdHandler(IProductRepository repo)
{
    public async Task<ProductDto> ExecuteAsync(int id, CancellationToken cancellationToken = default)
        => await repo.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Product {id} not found.", "PRODUCT_NOT_FOUND");
}
