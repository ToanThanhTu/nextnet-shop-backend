# SubCategories module

Second-level taxonomy under a Category. Same shape as Categories: public reads, admin writes, owns image bytes.

For DDD pattern rules see the parent [`net-backend/CLAUDE.md`](../../CLAUDE.md). For the cross-module landscape see [`Modules/CLAUDE.md`](../CLAUDE.md).

## Files at this level

| File | Purpose |
|---|---|
| `SubCategoriesController.cs` | HTTP adapter |
| `SubCategoriesModule.cs` | DI registration |
| `Domain/ISubCategoryRepository.cs` | Persistence contract; includes `GetImageAsync` |
| `Infrastructure/EfSubCategoryRepository.cs` | EF implementation |
| `Application/Queries/{ListSubCategoriesHandler, GetSubCategoryByIdHandler}.cs` | Read use cases |
| `Application/Commands/{CreateSubCategoryHandler, UpdateSubCategoryHandler, DeleteSubCategoryHandler}.cs` | Write use cases |
| `Contracts/{SubCategoryDto, CreateSubCategoryRequest, UpdateSubCategoryRequest}.cs` | API surface |

## Routes

| Method | Path | Auth | Handler |
|---|---|---|---|
| GET | `/subcategories` | public | `ListSubCategoriesHandler` |
| GET | `/subcategories/{id:int}` | public | `GetSubCategoryByIdHandler` |
| GET | `/subcategories/{id:int}/image` | public | `ISubCategoryRepository.GetImageAsync` directly |
| POST | `/subcategories` | Admin | `CreateSubCategoryHandler` |
| PUT | `/subcategories/{id:int}` | Admin | `UpdateSubCategoryHandler` |
| DELETE | `/subcategories/{id:int}` | Admin | `DeleteSubCategoryHandler` |

## Module-specific notes

- **Belongs to a Category**. Every SubCategory has a `CategoryId` FK; the create/update requests must reference an existing category, or the EF save throws and the global handler maps it to a 4xx.
- **List endpoint returns flat list, not nested**. The frontend joins by `categoryId` client-side. Don't change to a tree response without coordinating with the frontend `categories` module.
- Same image-endpoint pattern as Categories: byte stream bypasses the Application layer, served directly from the repository.
