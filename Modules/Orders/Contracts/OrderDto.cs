using net_backend.Data.Types;

namespace net_backend.Modules.Orders.Contracts;

public record OrderDto(
    int Id,
    DateTime OrderDate,
    decimal TotalPrice,
    string Status,
    List<OrderItemDto> OrderItems)
{
    public static OrderDto FromEntity(Order order) => new(
        order.Id,
        order.OrderDate,
        order.TotalPrice,
        order.Status,
        order.OrderItems?
            .Select(OrderItemDto.FromEntity)
            .ToList() ?? []);
}
