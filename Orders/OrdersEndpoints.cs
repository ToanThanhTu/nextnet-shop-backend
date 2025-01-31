using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using net_backend.Data.Types;

namespace net_backend.Orders;
public static class OrdersEndpoints
{
    public static void RegisterOrdersEndpoints(this WebApplication app)
    {
        var orders = app.MapGroup("/orders");

        orders.MapGet("/user/{userId}", GetOrders);
        orders.MapPost("/user/{userId}", PlaceOrder);
        orders.MapPut("/", UpdateOrder);

        static async Task<IResult> GetOrders(int userId, AppDbContext db)
        {
            var orders = await db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .Select(o => new OrderDTO
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status,
                    OrderItems = o.OrderItems!
                        .Select(oi => new OrderItemDTO
                        {
                            Id = oi.Id,
                            ProductId = oi.ProductId,
                            Quantity = oi.Quantity,
                            Price = oi.Price,
                            Product = oi.Product ?? null,
                        }).ToList()
                })
                .ToArrayAsync();

            return TypedResults.Ok(orders);
        }

        static async Task<IResult> PlaceOrder(int userId, [FromBody] CartItem[] cartItems, AppDbContext db)
        {
            // Validate the request cartItems from client with cartItems from the server
            var cartItemsServer = await db.CartItems
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Product)
                .ToListAsync();

            var isValid = cartItems.All(clientItem =>
                cartItemsServer.Any(serverItem =>
                    serverItem.ProductId == clientItem.ProductId &&
                    serverItem.Quantity == clientItem.Quantity &&
                    (serverItem.Product != null && serverItem.Product.SalePrice == clientItem.Product?.SalePrice)));

            if (!isValid)
            {
                return TypedResults.BadRequest("Cart items do not match the server. Please refresh your cart.");
            }

            // If valid, create and save the order
            var order = new Order
            {
                UserId = userId,
                TotalPrice = cartItems.Sum(ci => ci.Product != null ? ci.Product.SalePrice * ci.Quantity : 0),
            };

            await db.Orders.AddAsync(order);
            await db.SaveChangesAsync();

            var orderItems = cartItems.Select(ci => new OrderItem
            {
                OrderId = order.Id,
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                Price = ci.Product != null ? ci.Product.SalePrice : 0,
            });

            await db.OrderItems.AddRangeAsync(orderItems);
            await db.SaveChangesAsync();

            // Remove cart items from the server
            db.CartItems.RemoveRange(cartItemsServer);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/orders/{order.Id}", new
            {
                order,
                orderItems
            });
        }

        static async Task<IResult> UpdateOrder(Order order, AppDbContext db)
        {
            db.Orders.Update(order);
            await db.SaveChangesAsync();

            return TypedResults.NoContent();
        }
    }
}
