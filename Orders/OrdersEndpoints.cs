using Microsoft.EntityFrameworkCore;
using net_backend.Data.Types;

namespace net_backend.Order;
public static class OrdersEndpoints
{
    public static void RegisterOrdersEndpoints(this WebApplication app)
    {
        var orders = app.MapGroup("/orders");

        orders.MapGet("/user/{userId}", GetOrders);
        orders.MapPost("/", AddOrder);
        orders.MapPut("/", UpdateOrder);

        static async Task<IResult> GetOrders(int userId, AppDbContext db)
        {
            var orders = await db.Orders
                .Where(o => o.UserId == userId)
                .ToArrayAsync();

            return Results.Ok(orders);
        }

        static async Task<IResult> AddOrder(Order order, AppDbContext db)
        {
            await db.Orders.AddAsync(order);
            await db.SaveChangesAsync();
            return Results.Created($"/orders/{order.Id}", order);
        }
    }
