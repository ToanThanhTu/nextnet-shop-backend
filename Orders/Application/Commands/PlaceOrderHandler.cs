using net_backend.Orders.Contracts;
using net_backend.Orders.Domain;

namespace net_backend.Orders.Application.Commands;

/// <summary>
/// Thin handler that delegates to the OrderPlacement domain service.
/// All business logic (validation, stock, transaction) lives there;
/// this class is just a use-case-shaped seam for the controller.
/// </summary>
public class PlaceOrderHandler(OrderPlacement placement)
{
    public async Task<OrderDto> ExecuteAsync(
        int userId,
        PlaceOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await placement.PlaceFromCartAsync(userId, request.Items, cancellationToken);
        return OrderDto.FromEntity(order);
    }
}
