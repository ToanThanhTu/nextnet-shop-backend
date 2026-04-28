# Categories module

Top-level product taxonomy. Public read endpoints; admin-only writes. Owns the binary image bytes for category thumbnails, served as `image/jpeg` via a dedicated endpoint.

For DDD pattern rules and routing conventions see the parent [`net-backend/CLAUDE.md`](../../CLAUDE.md). For the cross-module landscape see [`Modules/CLAUDE.md`](../CLAUDE.md).

## Files at this level

| File | Purpose |
|---|---|
| `CategoriesController.cs` | HTTP adapter; one action per use case |
| `CategoriesModule.cs` | DI registration extension method |
| `Domain/ICategoryRepository.cs` | Persistence contract; includes `GetImageAsync` |
| `Infrastructure/EfCategoryRepository.cs` | EF implementation |
| `Application/Queries/{ListCategoriesHandler, GetCategoryByIdHandler}.cs` | Read use cases |
| `Application/Commands/{CreateCategoryHandler, UpdateCategoryHandler, DeleteCategoryHandler}.cs` | Write use cases |
| `Contracts/{CategoryDto, CreateCategoryRequest, UpdateCategoryRequest}.cs` | API surface types |

## Routes

| Method | Path | Auth | Handler |
|---|---|---|---|
| GET | `/categories` | public | `ListCategoriesHandler` |
| GET | `/categories/{id:int}` | public | `GetCategoryByIdHandler` |
| GET | `/categories/{id:int}/image` | public | `ICategoryRepository.GetImageAsync` directly (no handler) |
| POST | `/categories` | Admin | `CreateCategoryHandler` |
| PUT | `/categories/{id:int}` | Admin | `UpdateCategoryHandler` |
| DELETE | `/categories/{id:int}` | Admin | `DeleteCategoryHandler` |

## Module-specific notes

- **Image endpoint bypasses the Application layer**. Returning a raw `byte[]` doesn't fit the handler-returns-DTO pattern, so the controller calls the repository directly. Keep this pattern only for byte-stream responses.
- **`Projection`** on `CategoryDto` is used by `ListCategoriesHandler` so the SQL `SELECT` excludes the `byte[] Image` column. Important: list queries that don't need the image must never load it (the column is large and EF won't lazy-load by default in tracking-disabled queries).
- **Cascade**: `Categories` is the parent of `SubCategories`. Deleting a category that has subcategories will fail at the DB level (FK constraint). The `DeleteCategoryHandler` doesn't cascade explicitly; rely on the DB error and surface it as a `ConflictException` if needed.
