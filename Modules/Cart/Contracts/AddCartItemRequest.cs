using System.ComponentModel.DataAnnotations;

namespace net_backend.Modules.Cart.Contracts;

/// <summary>
/// POST /cart/items body. Adds <c>Quantity</c> units of the product to the
/// cart; if the product is already in the cart, increments the existing
/// quantity instead of creating a duplicate row.
/// </summary>
public record AddCartItemRequest(
    [Required, Range(1, int.MaxValue)] int ProductId,
    [Required, Range(1, 100)] int Quantity);
