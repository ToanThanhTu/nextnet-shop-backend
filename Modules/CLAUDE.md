# Modules — bounded contexts

Per-feature folders implementing the DDD layered pattern. Cross-cutting code lives outside `Modules/` (in `Common/`, `Configuration/`, `Data/`). For the layer rules, route conventions, exception hierarchy, and "adding a new feature" recipe, see the parent [`net-backend/CLAUDE.md`](../CLAUDE.md).

This file documents only what's specific to **the cross-module landscape**: which modules exist, which depend on which, and which authorization policies apply where.

## Modules in this codebase

| Module | Public route prefix | Auth model | Notes |
|---|---|---|---|
| [Categories](Categories/CLAUDE.md) | `/categories` | Public reads, admin writes | Owns category image bytes (served via `/categories/{id}/image`) |
| [SubCategories](SubCategories/CLAUDE.md) | `/subcategories` | Public reads, admin writes | Belongs to a Category |
| [Products](Products/CLAUDE.md) | `/products` | Public reads, admin writes | Largest module; many specialised list endpoints |
| [Cart](Cart/CLAUDE.md) | `/cart` | Authenticated; user-scoped | Operates on caller's own cart only |
| [Orders](Orders/CLAUDE.md) | `/orders` | Authenticated for reads/place; admin for status updates | Domain service `OrderPlacement` owns the multi-aggregate transaction |
| [Users](Users/CLAUDE.md) | `/users` | Mixed (public register/login, authed `me`, admin list) | Owns JWT issuance via `JwtTokenHelper` |

## Cross-module dependency graph

Modules import from each other only via Domain interfaces — never via concrete EF repositories or handlers. The current dependencies:

```
Orders   ─── depends on ──► Cart (reads CartItems for placement)
Orders   ─── depends on ──► Products (decrements stock)
Cart     ─── depends on ──► Products (validates product exists, reads price)
Products ─── depends on ──► Categories + SubCategories (FK relationships)
SubCategories ─── depends on ──► Categories (FK)
Users    ─── independent
```

`OrderPlacement.PlaceFromCartAsync` is the only place where three aggregates mutate together; that's why it's a domain service with an explicit transaction boundary.

## Module registration

Each module exposes a static extension method on `IServiceCollection` that registers its repository, handlers, and any domain services. The list lives in `ConfigureServices.AddServices`:

```csharp
builder.AddCategoriesFeature();
builder.AddSubCategoriesFeature();
builder.AddProductsFeature();
builder.AddCartFeature();
builder.AddOrdersFeature();
builder.AddUsersFeature();
```

Adding a new module = a new `Modules/<Feature>/` folder + one new line here. Nothing else changes in startup wiring.

## Authorization across modules

The `"Admin"` policy is defined once in `Configuration/AuthConfiguration.AddAuthorizationPolicies` and used per-method via `[Authorize(Policy = "Admin")]`. No module declares its own policies. If you need a new policy (e.g. `"Partner"`), add it there and reference it from controllers.
