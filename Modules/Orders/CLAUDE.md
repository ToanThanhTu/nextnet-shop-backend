# Orders module

Order placement and history. Authenticated for the user-facing endpoints; admin-only for status updates. Hosts the canonical multi-aggregate domain service in this codebase: `OrderPlacement`.

For DDD pattern rules see the parent [`net-backend/CLAUDE.md`](../../CLAUDE.md). For the cross-module landscape see [`Modules/CLAUDE.md`](../CLAUDE.md).

## Files at this level

| File | Purpose |
|---|---|
| `OrdersController.cs` | HTTP adapter; class-level `[Authorize]` |
| `OrdersModule.cs` | DI registration (registers `OrderPlacement` as scoped) |
| `Domain/IOrderRepository.cs` | Persistence contract |
| `Domain/OrderPlacement.cs` | Multi-aggregate domain service for placing an order |
| `Infrastructure/EfOrderRepository.cs` | EF implementation |
| `Application/Queries/GetMyOrdersHandler.cs` | List the caller's orders |
| `Application/Commands/{PlaceOrderHandler, UpdateOrderStatusHandler}.cs` | Write use cases |
| `Contracts/{OrderDto, OrderItemDto, PlaceOrderRequest, UpdateOrderStatusRequest}.cs` | API surface |

## Routes

| Method | Path | Auth | Handler | Notes |
|---|---|---|---|---|
| GET | `/orders` | `[Authorize]` | `GetMyOrdersHandler` | Returns caller's orders only |
| POST | `/orders` | `[Authorize]` | `PlaceOrderHandler` | Wraps `OrderPlacement.PlaceFromCartAsync` |
| PUT | `/orders/{id:int}/status` | `[Authorize(Policy = "Admin")]` | `UpdateOrderStatusHandler` | Admin-only state transitions |

## OrderPlacement: the canonical transactional domain service

`OrderPlacement.PlaceFromCartAsync` is the example domain service this codebase uses to demonstrate multi-aggregate writes. Every comparable use case in the future (refund, cancellation, reservation) should follow the same pattern.

**What it does, atomically inside `BeginTransactionAsync`:**

1. **Verify cart parity**: read the server's cart for the user, compare to the client-supplied items. If a product is missing or the quantity differs, throw `ValidationException("CART_MISMATCH")`. Catches stale local state and price-changes-since-add.
2. **Decrement stock**: load each product, check `Stock >= requested`, decrement. Concurrency token (a row-version) prevents two simultaneous orders from over-selling the same SKU.
3. **Create the Order + OrderItems**: capture the price-at-order-time per item; never trust the client's price.
4. **Clear the cart items** that became part of the order.
5. **Commit** if all four succeeded; rollback on any throw.

**Why this is a domain service, not a handler**: the operation crosses Cart, Product, and Order aggregates. The transaction boundary lives at the use-case level, not at the repository level. Per the project's DDD rule, multi-aggregate logic gets its own class named for the business action — never `OrderService`.

## Module-specific notes

- **Status transitions** are not yet validated as a state machine. `UpdateOrderStatusHandler` accepts any string. If you add `Pending → Confirmed → Shipped → Delivered` enforcement, model it in the domain layer (a `OrderStatus` value object with `CanTransitionTo`), not in the controller.
- **No order cancellation** today. Adding it = a new domain service `OrderCancellation` that reverses stock + refunds (when payment is wired in).
- **Email/notifications** on order placement are not implemented. The handler returns the order DTO; the controller `CreatedAtAction`s it. Wiring SendGrid or similar would happen as a domain event published from `OrderPlacement` after the transaction commits.
- **Order status updates do not notify the user**. Same deferred work as above.
