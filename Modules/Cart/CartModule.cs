using net_backend.Modules.Cart.Application.Commands;
using net_backend.Modules.Cart.Application.Queries;
using net_backend.Modules.Cart.Domain;
using net_backend.Modules.Cart.Infrastructure;

namespace net_backend.Modules.Cart;

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
