using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using net_backend.Common.Auth;
using net_backend.Modules.Orders.Application.Commands;
using net_backend.Modules.Orders.Application.Queries;
using net_backend.Modules.Orders.Contracts;

namespace net_backend.Modules.Orders;

/// <summary>
/// Order endpoints. The user id always comes from the JWT NameIdentifier
/// claim — never from a path or body parameter.
/// </summary>
[ApiController]
[Route("orders")]
[Authorize]
public class OrdersController(
    GetMyOrdersHandler getMyOrdersHandler,
    PlaceOrderHandler placeOrderHandler,
    UpdateOrderStatusHandler updateStatusHandler) : ControllerBase
{
    [HttpGet("")]
    public async Task<ActionResult<List<OrderDto>>> GetMyOrders(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        return Ok(await getMyOrdersHandler.ExecuteAsync(userId, cancellationToken));
    }

    [HttpPost("")]
    public async Task<ActionResult<OrderDto>> Place(
        [FromBody] PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var order = await placeOrderHandler.ExecuteAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetMyOrders), order);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        await updateStatusHandler.ExecuteAsync(id, request, cancellationToken);
        return NoContent();
    }
}
