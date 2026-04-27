using net_backend.Data.Types;
using net_backend.Modules.Products.Contracts;
using net_backend.Modules.Products.Domain;

namespace net_backend.Modules.Products.Application.Commands;

public class CreateProductHandler(IProductRepository repo)
{
    public async Task<ProductDto> ExecuteAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            Sale = (byte)request.Sale,
            Stock = (byte)request.Stock,
            SubCategoryId = request.SubCategoryId,
        };

        var saved = await repo.AddAsync(product, cancellationToken);
        return ProductDto.FromEntity(saved);
    }
}
