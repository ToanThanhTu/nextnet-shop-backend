using Microsoft.EntityFrameworkCore;
using net_backend.Common.Exceptions;
using net_backend.Data.Types;
using net_backend.Modules.Orders.Contracts;

namespace net_backend.Modules.Orders.Domain;

/// <summary>
/// Domain service for the "place an order from the user's cart" use case.
/// Coordinates three aggregates atomically: Cart, Product (stock), Order.
///
/// Named for the business action, per ~/.claude/rules/backend-ddd.md
/// (no FooService / FooManager). Future use cases that touch orders get
/// their own domain services: OrderCancellation, RefundPolicy, etc.
///
/// Takes AppDbContext directly because transaction control is an
/// infrastructure concern that's awkward to abstract behind the
/// repository interfaces. The hand-off is honest: this class owns the
/// transaction boundary; everything else stays in repos.
/// </summary>
public class OrderPlacement(AppDbContext db)
{
    public async Task<Order> PlaceFromCartAsync(
        int userId,
        IReadOnlyList<PlaceOrderItem> requested,
        CancellationToken cancellationToken = default)
    {
        if (requested.Count == 0)
        {
            throw new ValidationException("Cannot place an order with no items.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        // 1. Verify the client's view of the cart matches the server's.
        //    Catches stale local state and price changes since the user added
        //    the item.
        var serverCart = await db.CartItems
            .Where(ci => ci.UserId == userId)
            .Include(ci => ci.Product)
            .ToListAsync(cancellationToken);

        foreach (var item in requested)
        {
            var serverItem = serverCart.FirstOrDefault(ci => ci.ProductId == item.ProductId);
            if (serverItem is null)
                throw new ValidationException(
                    $"Product {item.ProductId} is not in your cart.",
                    "CART_MISMATCH");
            if (serverItem.Quantity != item.Quantity)
                throw new ValidationException(
                    $"Cart quantity for product {item.ProductId} differs; please refresh your cart.",
                    "CART_MISMATCH");
            if (serverItem.Product is null
                || serverItem.Product.SalePrice != item.ExpectedSalePrice)
                throw new ValidationException(
                    $"Price for product {item.ProductId} has changed; please refresh your cart.",
                    "PRICE_CHANGED");
        }

        // 2. Decrement stock per product. Loading by Id list = single query;
        //    EF tracks the entities so SaveChanges below persists the updates.
        var productIds = requested.Select(i => i.ProductId).Distinct().ToArray();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        foreach (var item in requested)
        {
            var product = products.First(p => p.Id == item.ProductId);
            if (product.Stock < item.Quantity)
                throw new ConflictException(
                    $"Not enough stock for {product.Title}.",
                    "INSUFFICIENT_STOCK");

            product.Stock = (byte)(product.Stock - item.Quantity);
            product.Sold = (product.Sold ?? 0) + item.Quantity;
        }

        // 3. Create the order with line items in one go. EF cascade-inserts
        //    the OrderItems via the navigation property.
        var order = new Order
        {
            UserId = userId,
            TotalPrice = requested.Sum(i => i.ExpectedSalePrice * i.Quantity),
            OrderItems = requested.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.ExpectedSalePrice,
            }).ToList(),
        };
        db.Orders.Add(order);

        // 4. Clear the user's cart in the same transaction.
        db.CartItems.RemoveRange(serverCart);

        // 5. One SaveChanges covers the inserts + updates + deletes. The
        //    transaction commits only if everything succeeds.
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return order;
    }
}
