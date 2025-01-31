using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Namotion.Reflection;
using net_backend.Data.Types;

namespace net_backend.Cart;
public static class CartEndpoints
{
    public static void RegisterCartEndpoints(this WebApplication app)
    {
        var cart = app.MapGroup("/cart");

        cart.MapGet("/user/{userId}", GetCartItems);
        cart.MapPost("/", AddCartItem);
        cart.MapPut("/", UpdateCartItem);
        cart.MapDelete("/item", RemoveCartItem);
        cart.MapDelete("/user/{userId}", ClearCart);
        cart.MapPost("/sync/{userId}", SyncCart);

        static async Task<IResult> GetCartItems(int userId, AppDbContext db)
        {
            var cartItems = await db.CartItems
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Product)
                .ToArrayAsync();

            return TypedResults.Ok(cartItems);
        }

        static async Task<IResult> AddCartItem([FromBody] CartItemDTO cartItemDto, AppDbContext db)
        {
            // Find existing cart item
            var cartItem = await db.CartItems
                .Where(ci => ci.UserId == cartItemDto.UserId && ci.ProductId == cartItemDto.ProductId)
                .FirstOrDefaultAsync();

            if (cartItem != null)
            {
                // Update quantity if cart item already exists
                cartItem.Quantity += cartItemDto.Quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    UserId = cartItemDto.UserId,
                    ProductId = cartItemDto.ProductId,
                    Quantity = cartItemDto.Quantity,
                };
                await db.CartItems.AddAsync(cartItem);
            }

            await db.SaveChangesAsync();

            cartItem.Product = await db.Products.FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);

            return TypedResults.Created($"/cart/item/{cartItem.Id}", cartItem);
        }

        static async Task<IResult> UpdateCartItem([FromBody] CartItemDTO cartItemDto, AppDbContext db)
        {
            var existingCartItem = await db.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == cartItemDto.UserId && ci.ProductId == cartItemDto.ProductId);

            if (existingCartItem == null)
            {
                return TypedResults.NotFound();
            }

            existingCartItem.Quantity = cartItemDto.Quantity;

            await db.SaveChangesAsync();

            existingCartItem.Product = await db.Products.FirstOrDefaultAsync(p => p.Id == existingCartItem.ProductId);

            return TypedResults.Ok(existingCartItem);
        }

        static async Task<IResult> RemoveCartItem([FromBody] CartItemDTO cartItemDto, AppDbContext db)
        {
            var cartItem = await db.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == cartItemDto.UserId && ci.ProductId == cartItemDto.ProductId);

            if (cartItem == null)
            {
                return TypedResults.NotFound();
            }

            db.CartItems.Remove(cartItem);
            await db.SaveChangesAsync();

            return TypedResults.Ok(cartItemDto);
        }

        static async Task<IResult> ClearCart(int userId, AppDbContext db)
        {
            var cartItems = await db.CartItems
                .Where(ci => ci.UserId == userId)
                .ToArrayAsync();

            db.CartItems.RemoveRange(cartItems);
            await db.SaveChangesAsync();

            return TypedResults.NoContent();
        }

        static async Task<IResult> SyncCart(int userId, [FromBody] CartItemDTO[] clientCartItems, AppDbContext db)
        {
            var serverCartItems = await db.CartItems
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Product)
                .ToArrayAsync();

            if (serverCartItems.Length == 0 && clientCartItems.Length == 0)
            {
                // No cart items to sync, return empty list
                return TypedResults.Ok(new List<CartItem>());
            }

            if (serverCartItems.Length == 0)
            {
                // No server cart items, add all client cart items
                var newCartItems = clientCartItems.Select(ci => new CartItem
                {
                    UserId = userId,
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                }).ToList();

                await db.CartItems.AddRangeAsync(newCartItems);
                await db.SaveChangesAsync();

                foreach (var item in newCartItems)
                {
                    item.Product = await db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                }

                return TypedResults.Created($"/cart/user/{userId}", newCartItems);
            }

            if (clientCartItems.Length == 0)
            {
                // No client cart items, return server cart items
                return TypedResults.Ok(serverCartItems);
            }

            // Both server and client cart items exist, remove all server cart items and add all client cart items
            db.CartItems.RemoveRange(serverCartItems);

            var updatedCartItems = clientCartItems.Select(ci => new CartItem
            {
                UserId = userId,
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
            }).ToList();

            await db.CartItems.AddRangeAsync(updatedCartItems);
            await db.SaveChangesAsync();

            foreach (var item in updatedCartItems)
            {
                item.Product = await db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
            }

            return TypedResults.Created($"/cart/user/{userId}", updatedCartItems);
        }
    }
}
