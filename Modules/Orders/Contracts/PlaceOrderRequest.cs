using System.ComponentModel.DataAnnotations;

namespace net_backend.Modules.Orders.Contracts;

/// <summary>
/// POST /orders body. The client sends each cart item with the price it
/// believes it's paying; the server verifies these match the user's actual
/// cart and the current product prices, so a stale client-side cart can't
/// commit a wrong total.
/// </summary>
public record PlaceOrderRequest(
    [Required, MinLength(1)] List<PlaceOrderItem> Items);

public record PlaceOrderItem(
    [Required, Range(1, int.MaxValue)] int ProductId,
    [Required, Range(1, 100)] int Quantity,
    [Required, Range(0, double.MaxValue)] decimal ExpectedSalePrice);
