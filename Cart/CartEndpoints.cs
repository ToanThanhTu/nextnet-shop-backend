using Microsoft.EntityFrameworkCore;
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
        cart.MapDelete("/item/{id}", RemoveCartItem);
        cart.MapDelete("/user/{userId}", ClearCart);

        static async Task<IResult> GetCartItems(int userId, AppDbContext db)
        {
            var cartItems = await db.CartItems
                .Where(ci => ci.UserId == userId)
                .ToArrayAsync();

            return Results.Ok(cartItems);
        }

        static async Task<IResult> AddCartItem(CartItem cartItem, AppDbContext db)
        {
            await db.CartItems.AddAsync(cartItem);
            await db.SaveChangesAsync();

            return Results.Created($"/cart/item/{cartItem.Id}", cartItem);
        }

        static async Task<IResult> UpdateCartItem(CartItem cartItem, AppDbContext db)
        {
            db.CartItems.Update(cartItem);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }

        static async Task<IResult> RemoveCartItem(int id, AppDbContext db)
        {
            var cartItem = await db.CartItems.FindAsync(id);

            if (cartItem is null)
            {
                return Results.NotFound();
            }

            db.CartItems.Remove(cartItem);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }

        static async Task<IResult> ClearCart(int userId, AppDbContext db)
        {
            var cartItems = await db.CartItems
                .Where(ci => ci.UserId == userId)
                .ToArrayAsync();

            db.CartItems.RemoveRange(cartItems);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }
    }
}
