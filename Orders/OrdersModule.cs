using net_backend.Orders.Application.Commands;
using net_backend.Orders.Application.Queries;
using net_backend.Orders.Domain;
using net_backend.Orders.Infrastructure;

namespace net_backend.Orders;

public static class OrdersModule
{
    public static WebApplicationBuilder AddOrdersFeature(this WebApplicationBuilder builder)
    {
        // Repository
        builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();

        // Domain service: scoped because it depends on AppDbContext (per-request).
        builder.Services.AddScoped<OrderPlacement>();

        // Handlers
        builder.Services.AddScoped<GetMyOrdersHandler>();
        builder.Services.AddScoped<PlaceOrderHandler>();
        builder.Services.AddScoped<UpdateOrderStatusHandler>();

        return builder;
    }
}
