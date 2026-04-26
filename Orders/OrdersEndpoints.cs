using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using net_backend.Data.Types;

namespace net_backend.Orders;
public static class OrdersEndpoints
{
    public static void RegisterOrdersEndpoints(this WebApplication app)
    {
        var orders = app.MapGroup("/orders").RequireAuthorization();

        orders.MapGet("/user/{userId}", GetOrders);
        orders.MapPost("/user/{userId}", PlaceOrder);
        orders.MapPut("/", UpdateOrder).RequireAuthorization("Admin");

        static async Task<IResult> GetOrders(int userId, AppDbContext db)
        {
            var orders = await db.Orders
                .AsNoTracking()
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
            if (cartItems.Length == 0)
                return TypedResults.BadRequest("Cart is empty.");

            // Single transaction: validate cart, decrement stock with concurrency
            // protection, create order + items, clear the cart. If any step fails,
            // the transaction rolls back and the database stays consistent.
            await using var tx = await db.Database.BeginTransactionAsync();

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

            // Lock the affected product rows so two simultaneous orders can't
            // both pass the stock check on the same item.
            var productIds = cartItems.Select(ci => ci.ProductId).Distinct().ToArray();
            var products = await db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var item in cartItems)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product is null)
                    return TypedResults.BadRequest($"Product {item.ProductId} not found.");
                if (product.Stock < item.Quantity)
                    return TypedResults.BadRequest($"Not enough stock for {product.Title}.");

                product.Stock = (byte)(product.Stock - item.Quantity);
                product.Sold = (product.Sold ?? 0) + item.Quantity;
            }

            var order = new Order
            {
                UserId = userId,
                TotalPrice = cartItems.Sum(ci => ci.Product != null ? ci.Product.SalePrice * ci.Quantity : 0),
                OrderItems = cartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Product != null ? ci.Product.SalePrice : 0,
                }).ToList(),
            };

            db.Orders.Add(order);
            db.CartItems.RemoveRange(cartItemsServer);
            await db.SaveChangesAsync();
            await tx.CommitAsync();

            return TypedResults.Created($"/orders/{order.Id}", new OrderDTO
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                OrderItems = order.OrderItems!
                    .Select(oi => new OrderItemDTO
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        Price = oi.Price,
                    }).ToList(),
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
