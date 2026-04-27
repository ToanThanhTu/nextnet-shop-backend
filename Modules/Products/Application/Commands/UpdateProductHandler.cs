using net_backend.Common.Exceptions;
using net_backend.Modules.Products.Contracts;
using net_backend.Modules.Products.Domain;

namespace net_backend.Modules.Products.Application.Commands;

public class UpdateProductHandler(IProductRepository repo)
{
    public async Task ExecuteAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await repo.GetEntityByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Product {id} not found.", "PRODUCT_NOT_FOUND");

        product.Title = request.Title;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Sale = (byte)request.Sale;
        product.Stock = (byte)request.Stock;

        await repo.UpdateAsync(product, cancellationToken);
    }
}
