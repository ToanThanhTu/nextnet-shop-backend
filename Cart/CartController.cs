using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using net_backend.Cart.Application.Commands;
using net_backend.Cart.Application.Queries;
using net_backend.Cart.Contracts;
using net_backend.Common.Auth;

namespace net_backend.Cart;

/// <summary>
/// Cart endpoints scoped to the authenticated user. The user id always
/// comes from the JWT NameIdentifier claim (User.GetRequiredUserId()),
/// never from a path parameter — that lets one user act on another's cart.
/// </summary>
[ApiController]
[Route("cart")]
[Authorize]
public class CartController(
    GetMyCartHandler getHandler,
    AddCartItemHandler addHandler,
    UpdateCartItemHandler updateHandler,
    RemoveCartItemHandler removeHandler,
    ClearCartHandler clearHandler,
    SyncCartHandler syncHandler) : ControllerBase
{
    [HttpGet("")]
    public async Task<ActionResult<List<CartItemDto>>> GetMyCart(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        return Ok(await getHandler.ExecuteAsync(userId, cancellationToken));
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartItemDto>> AddItem(
        [FromBody] AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var item = await addHandler.ExecuteAsync(userId, request, cancellationToken);
        return Ok(item);
    }

    [HttpPut("items")]
    public async Task<ActionResult<CartItemDto>> UpdateItem(
        [FromBody] UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var item = await updateHandler.ExecuteAsync(userId, request, cancellationToken);
        return Ok(item);
    }

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveItem(int productId, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        await removeHandler.ExecuteAsync(userId, productId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("")]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        await clearHandler.ExecuteAsync(userId, cancellationToken);
        return NoContent();
    }

    [HttpPost("sync")]
    public async Task<ActionResult<List<CartItemDto>>> Sync(
        [FromBody] SyncCartRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var items = await syncHandler.ExecuteAsync(userId, request, cancellationToken);
        return Ok(items);
    }
}
