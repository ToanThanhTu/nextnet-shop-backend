using net_backend.Common.Exceptions;
using net_backend.Orders.Contracts;
using net_backend.Orders.Domain;

namespace net_backend.Orders.Application.Commands;

/// <summary>
/// Update an order's status. Admin-only at the controller layer.
/// Whitelists status values to keep "🦄" out of the field; a richer
/// state machine (pending -> paid -> shipped -> delivered) belongs on
/// the Order entity itself when we tackle the audit's W1.
/// </summary>
public class UpdateOrderStatusHandler(IOrderRepository repo)
{
    private static readonly string[] AllowedStatuses =
        { "Pending", "Paid", "Shipped", "Delivered", "Cancelled" };

    public async Task ExecuteAsync(
        int orderId,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!AllowedStatuses.Contains(request.Status))
        {
            throw new ValidationException(
                $"Status '{request.Status}' is not one of: {string.Join(", ", AllowedStatuses)}.",
                "INVALID_STATUS");
        }

        var order = await repo.GetByIdAsync(orderId, cancellationToken)
            ?? throw new NotFoundException($"Order {orderId} not found.", "ORDER_NOT_FOUND");

        order.Status = request.Status;
        await repo.UpdateAsync(order, cancellationToken);
    }
}
