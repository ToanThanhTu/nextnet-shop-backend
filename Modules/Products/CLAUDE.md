# Products module

The largest bounded context in this codebase. Many specialised list endpoints (general, on-sale, bestsellers, top-deals, search, similar, personal recommendations) all dispatching to dedicated query handlers.

For DDD pattern rules see the parent [`net-backend/CLAUDE.md`](../../CLAUDE.md). For the cross-module landscape see [`Modules/CLAUDE.md`](../CLAUDE.md).

## Files at this level

| File | Purpose |
|---|---|
| `ProductsController.cs` | HTTP adapter; ~12 action methods |
| `ProductsModule.cs` | DI registration |
| `Domain/IProductRepository.cs` | Persistence contract |
| `Infrastructure/EfProductRepository.cs` | EF implementation |
| `Application/Queries/*` | Eight read handlers (see route table) |
| `Application/Commands/{CreateProductHandler, UpdateProductHandler, DeleteProductHandler}.cs` | Write use cases |
| `Contracts/{ProductDto, ProductWithHierarchyDto, ProductListPageDto, ProductListQuery, CreateProductRequest, UpdateProductRequest}.cs` | API surface |

## Routes

Read endpoints (all public):

| Method | Path | Handler | Returns |
|---|---|---|---|
| GET | `/products/all` | `ListProductsHandler` | `ProductListPageDto` (paginated) |
| GET | `/products/sales` | `ListOnSaleProductsHandler` | `ProductListPageDto` |
| GET | `/products/bestsellers` | `ListBestsellersHandler` | `ProductListPageDto` |
| GET | `/products/top-deals` | `GetTopDealsHandler` | `List<ProductDto>` |
| GET | `/products/search?query=...` | `SearchProductsHandler` | `List<ProductDto>` |
| GET | `/products/{id:int}` | `GetProductByIdHandler` | `ProductDto` |
| GET | `/products/by-slug/{slug}` | `GetProductBySlugHandler` | `ProductWithHierarchyDto` |
| GET | `/products/{id:int}/similar` | `GetSimilarProductsHandler` | `List<ProductDto>` |
| GET | `/products/personal-recommendations` | `GetPersonalRecommendationsHandler` | `List<ProductDto>` (uses JWT user if logged in) |
| GET | `/products/{id:int}/image` | repository direct | `image/jpeg` bytes |

Write endpoints (all `[Authorize(Policy = "Admin")]`):

| Method | Path | Handler |
|---|---|---|
| POST | `/products` | `CreateProductHandler` |
| PUT | `/products/{id:int}` | `UpdateProductHandler` |
| DELETE | `/products/{id:int}` | `DeleteProductHandler` |

## Module-specific notes

- **`ProductDto.Projection`** is the most-reused projection in the codebase. Every list query uses it so the SQL `SELECT` excludes `byte[] Image`. Loading the image column on a 100-product list page would be catastrophic for response time.
- **`ProductListQuery`** carries pagination + filter + sort. Bind via `[FromQuery]` so query-string params flow in directly.
- **`ProductWithHierarchyDto`** is the only place a product is returned with its category + subcategory inline, used on the product detail page so the frontend doesn't need three round-trips.
- **`GetPersonalRecommendationsHandler`** reads the JWT user if present, falls back to a generic recommendation set if anonymous. The controller doesn't gate on `[Authorize]`; the handler decides what to do based on the optional user id.
- **Stock decrement** is not in this module. It happens in `Modules/Orders/Domain/OrderPlacement` inside the order-placement transaction.
- **Slug uniqueness** is enforced by a unique index in the migration, not by application-level checking. A duplicate slug at create/update time surfaces as a DB error → `ConflictException` → 409.
