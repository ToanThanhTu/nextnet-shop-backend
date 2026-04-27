using net_backend.Cart.Application.Commands;
using net_backend.Cart.Application.Queries;
using net_backend.Cart.Domain;
using net_backend.Cart.Infrastructure;

namespace net_backend.Cart;

public static class CartModule
{
    public static WebApplicationBuilder AddCartFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICartRepository, EfCartRepository>();
        builder.Services.AddScoped<GetMyCartHandler>();
        builder.Services.AddScoped<AddCartItemHandler>();
        builder.Services.AddScoped<UpdateCartItemHandler>();
        builder.Services.AddScoped<RemoveCartItemHandler>();
        builder.Services.AddScoped<ClearCartHandler>();
        builder.Services.AddScoped<SyncCartHandler>();
        return builder;
    }
}
