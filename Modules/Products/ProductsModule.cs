using net_backend.Products.Application.Commands;
using net_backend.Products.Application.Queries;
using net_backend.Products.Domain;
using net_backend.Products.Infrastructure;

namespace net_backend.Products;

public static class ProductsModule
{
    public static WebApplicationBuilder AddProductsFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IProductRepository, EfProductRepository>();

        builder.Services.AddScoped<ListProductsHandler>();
        builder.Services.AddScoped<ListOnSaleProductsHandler>();
        builder.Services.AddScoped<ListBestsellersHandler>();
        builder.Services.AddScoped<GetTopDealsHandler>();
        builder.Services.AddScoped<SearchProductsHandler>();
        builder.Services.AddScoped<GetProductByIdHandler>();
        builder.Services.AddScoped<GetProductBySlugHandler>();
        builder.Services.AddScoped<GetSimilarProductsHandler>();
        builder.Services.AddScoped<GetPersonalRecommendationsHandler>();
        builder.Services.AddScoped<CreateProductHandler>();
        builder.Services.AddScoped<UpdateProductHandler>();
        builder.Services.AddScoped<DeleteProductHandler>();

        return builder;
    }
}
