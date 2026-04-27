using net_backend.Common.Exceptions;
using net_backend.Products.Domain;

namespace net_backend.Products.Application.Commands;

public class DeleteProductHandler(IProductRepository repo)
{
    public async Task ExecuteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await repo.GetEntityByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Product {id} not found.", "PRODUCT_NOT_FOUND");

        await repo.DeleteAsync(product, cancellationToken);
    }
}
