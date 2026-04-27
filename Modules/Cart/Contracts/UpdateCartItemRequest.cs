using System.ComponentModel.DataAnnotations;

namespace net_backend.Modules.Cart.Contracts;

/// <summary>
/// PUT /cart/items body. Sets <c>Quantity</c> as the new absolute value
/// (replaces the existing count, doesn't add).
/// </summary>
public record UpdateCartItemRequest(
    [Required, Range(1, int.MaxValue)] int ProductId,
    [Required, Range(1, 100)] int Quantity);
