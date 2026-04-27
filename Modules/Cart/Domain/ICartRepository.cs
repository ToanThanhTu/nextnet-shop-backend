using net_backend.Data.Types;

namespace net_backend.Modules.Cart.Domain;

/// <summary>
/// Cart aggregate's persistence contract. All queries and mutations are
/// scoped to a single user; methods take userId explicitly so handlers
/// never accidentally cross users.
/// </summary>
public interface ICartRepository
{
    /// <summary>List a user's cart items with Product eagerly loaded (one JOIN).</summary>
    Task<List<CartItem>> ListByUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find a specific (user, product) row, eagerly loading Product. Returns
    /// null if the item isn't in the cart.
    /// </summary>
    Task<CartItem?> GetByUserAndProductWithProductAsync(
        int userId, int productId, CancellationToken cancellationToken = default);

    /// <summary>Insert a new cart item and return it with Product populated.</summary>
    Task<CartItem> AddAsync(CartItem item, CancellationToken cancellationToken = default);

    /// <summary>Persist changes to a tracked cart item.</summary>
    Task UpdateAsync(CartItem item, CancellationToken cancellationToken = default);

    /// <summary>Remove a tracked cart item.</summary>
    Task DeleteAsync(CartItem item, CancellationToken cancellationToken = default);

    /// <summary>Remove every cart item for the given user.</summary>
    Task ClearByUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replace a user's cart contents atomically: delete all existing rows,
    /// insert the new ones, return the new list with Product loaded.
    /// </summary>
    Task<List<CartItem>> ReplaceForUserAsync(
        int userId,
        IEnumerable<(int ProductId, int Quantity)> items,
        CancellationToken cancellationToken = default);
}
