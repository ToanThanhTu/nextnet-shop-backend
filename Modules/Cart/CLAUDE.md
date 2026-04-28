# Cart module

User-scoped shopping cart. Every endpoint requires authentication and operates on the caller's own cart only — the user id always comes from the JWT, never from the path or body.

For DDD pattern rules see the parent [`net-backend/CLAUDE.md`](../../CLAUDE.md). For the cross-module landscape see [`Modules/CLAUDE.md`](../CLAUDE.md).

## Files at this level

| File | Purpose |
|---|---|
| `CartController.cs` | HTTP adapter; class-level `[Authorize]` |
| `CartModule.cs` | DI registration |
| `Domain/ICartRepository.cs` | Persistence contract |
| `Infrastructure/EfCartRepository.cs` | EF implementation |
| `Application/Queries/GetMyCartHandler.cs` | Reads the caller's cart |
| `Application/Commands/{AddCartItemHandler, UpdateCartItemHandler, RemoveCartItemHandler, ClearCartHandler, SyncCartHandler}.cs` | Write use cases |
| `Contracts/{CartItemDto, AddCartItemRequest, UpdateCartItemRequest, SyncCartRequest}.cs` | API surface |

## Routes

All require `[Authorize]` (class-level on the controller):

| Method | Path | Handler | Notes |
|---|---|---|---|
| GET | `/cart` | `GetMyCartHandler` | Returns `List<CartItemDto>` |
| POST | `/cart/items` | `AddCartItemHandler` | Idempotent on existing line: increments quantity if already present |
| PUT | `/cart/items` | `UpdateCartItemHandler` | Sets absolute quantity for one product |
| DELETE | `/cart/items/{productId:int}` | `RemoveCartItemHandler` | Removes one line |
| DELETE | `/cart` | `ClearCartHandler` | Removes all of caller's cart items |
| POST | `/cart/sync` | `SyncCartHandler` | Replaces the cart with a client-supplied list (used for guest → logged-in merge) |

## Module-specific notes

- **`User.GetRequiredUserId()`** is called in every action method. Don't accept a `userId` parameter for any cart endpoint, ever. The path `/cart/items/{userId}/{productId}` would be a security hole.
- **`SyncCartHandler`** is the only command that replaces the cart wholesale. Used when a guest who built a cart pre-login signs in: the frontend sends the local cart up, the server reconciles. Treat as idempotent and prefer it over multiple `Add` calls in a row.
- **Cart price** is recomputed at order placement, not stored. Never trust the cart's local price for the order total; `OrderPlacement.PlaceFromCartAsync` reads the current product price during the transaction.
- **Stock check happens at order placement, not on add-to-cart**. A user can add a product to their cart even if it's out of stock; the order will fail with `INSUFFICIENT_STOCK` at placement time. This is intentional — UX prefers "you can browse with the product in your cart" over "blocked at the cart action."
