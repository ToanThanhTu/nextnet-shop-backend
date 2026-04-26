# Backend conventions and gotchas

Stack-specific rules for `net-backend`. Project-wide conventions live in `../CLAUDE.md`.

## Architecture: feature modules with extension methods

Each feature is a folder at the root (`Categories/`, `Products/`, `Users/`, `Cart/`, `Orders/`). Each folder owns a single `*Endpoints.cs` file (Subcategories share the Categories folder).

The pattern is:

```csharp
public static class FooEndpoints
{
    public static void RegisterFooEndpoints(this WebApplication app)
    {
        var foo = app.MapGroup("/foo");
        foo.MapGet("/", GetAll);
        foo.MapGet("/{id}", GetById);
        // ...

        static async Task<IResult> GetAll(AppDbContext db) { ... }
        // ...
    }
}
```

Handlers are local static functions inside the `Register*Endpoints` method, declared after the route registrations. They take dependencies as parameters (DI is per-handler, not per-class).

`ConfigureApp.Configure` calls each `Register*Endpoints` extension after the middleware pipeline is set up.

## Routes are unprefixed

Routes do not use `/api/...`. They sit at the top level: `/categories`, `/products/all/`, `/users/login`, etc. Don't introduce an `/api` prefix without changing every existing route and the frontend's expectations.

## Return types: `IResult` + `TypedResults`

Handlers return `Task<IResult>` and use `TypedResults.Ok(...)`, `TypedResults.NotFound()`, `TypedResults.Created(uri, value)`, `TypedResults.BadRequest(message)`, etc. `TypedResults` over `Results` because:

- the return type is preserved at compile time (testable),
- response metadata is automatically attached for OpenAPI.

Don't mix the two styles in a single handler.

## DTOs over entities at the API boundary

Endpoints project entities into DTOs (`Data/Types/*DTO.cs`) before returning. Don't return raw `Product`, `User`, etc.; serialisation can leak fields (e.g. `PasswordHash`) and EF tracking annotations.

Use `.AsNoTracking()` for read-only queries and project directly into the DTO via `Select(...)` to avoid loading unused columns.

## Database

- DbContext: `Data/AppDbContext.cs`. All `DbSet`s + `OnModelCreating` configuration (table names, FKs, indexes).
- Tables use lowercase plural names; entity classes use PascalCase singular.
- Cascading deletes are configured for parent-child aggregates (User → Orders/CartItems, Category → SubCategories, etc.).
- Indexes: `Product.SubCategoryId`, `SubCategory.CategoryId`, `Order.UserId`, `OrderItem.OrderId`, `CartItem.UserId`.
- Connection string parsing: `ConfigureServices.AddDatabase` parses `postgres://...` URLs into Npgsql format. Don't use the EF Core conventional `Server=...;Database=...;` style here; the URL parsing logic expects the URL form.

## Authentication and authorization

JWT Bearer authentication is wired up (`ConfigureServices.AddJwtAuthentication`) and the middleware is enabled (`app.UseAuthentication()`). However:

- `app.UseAuthorization()` is **commented out** in `ConfigureApp.cs`.
- No endpoint calls `RequireAuthorization()`.

Effect: all endpoints are publicly accessible. JWTs are issued by `/users/login` (via `JwtTokenHelper`) but never required.

Before adding any endpoint that should be protected, do all three:

1. Uncomment `app.UseAuthorization();` in `ConfigureApp.cs`.
2. Add `.RequireAuthorization()` to the specific endpoint or group.
3. Verify with a request lacking a token: should return 401.

JWT signing key, issuer, and audience are currently hardcoded constants. Move these to configuration before changing prod.

## Configuration

Don't touch `process.env`-style env vars directly. Read from `builder.Configuration.GetConnectionString(...)` or `builder.Configuration["..."]` so dev/prod overrides work.

The `appsettings.json` connection string points at `localhost:15432` (the local Docker DB exposed to the host). Inside the backend container, the env var `ConnectionStrings__DATABASE_URL` overrides it to `db:5432` (compose network DNS). Keep both in sync if you change the local DB credentials.

## Migrations workflow

Migrations live in `Migrations/`. The baseline (`InitialCreate`) was generated after restoring a prod data dump; it is recorded as applied in `__EFMigrationsHistory` without ever running its `Up()` method. This keeps the local schema identical to prod's hand-crafted version.

When adding a new migration:

```bash
cd net-backend
dotnet ef migrations add <PascalCaseName>
# Review the generated Migrations/*.cs file BEFORE applying
dotnet ef database update
```

If a generated migration tries to recreate an existing table (because the model and the live schema disagree), don't apply it. Either reconcile the model to reality, or `dotnet ef migrations remove` and write the migration by hand.

## Adding a new endpoint module

1. Create `<Feature>/<Feature>Endpoints.cs` with a `Register<Feature>Endpoints(this WebApplication app)` extension.
2. Inside the extension, `var <feature> = app.MapGroup("/<feature>");` then register routes.
3. Add handlers as local static functions; project to DTOs.
4. Call `app.Register<Feature>Endpoints();` from `ConfigureApp.Configure`.
5. If new entities are involved: add the entity in `Data/Types/`, add the `DbSet` and `OnModelCreating` configuration in `AppDbContext`, then `dotnet ef migrations add Add<Feature>`.

## Things to avoid

- **Don't hardcode secrets or credentials.** The current JWT signing key is hardcoded; that's a known wart, not a pattern to copy.
- **Don't return entities directly** from endpoints; always project to a DTO.
- **Don't use `Results.Ok(...)`** when `TypedResults.Ok(...)` is available; you lose OpenAPI metadata.
- **Don't add a global `app.UseExceptionHandler`** without thinking through the error-response shape; the API currently has none, so any added one becomes the contract.
- **Don't share `Endpoints` registrations across features.** One feature, one file, one `MapGroup`.
