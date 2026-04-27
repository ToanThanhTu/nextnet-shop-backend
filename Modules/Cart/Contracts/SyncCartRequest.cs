using System.ComponentModel.DataAnnotations;

namespace net_backend.Modules.Cart.Contracts;

/// <summary>
/// POST /cart/sync body. Replaces the user's server cart with these items.
/// Used by the frontend to merge a guest-session cart on sign-in.
/// </summary>
public record SyncCartRequest(
    [Required] List<SyncCartItem> Items);

public record SyncCartItem(
    [Required, Range(1, int.MaxValue)] int ProductId,
    [Required, Range(1, 100)] int Quantity);
