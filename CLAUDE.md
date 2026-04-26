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
        foo.MapPost("/", Create).RequireAuthorization("Admin");
        // ...

        static async Task<IResult> GetAll(AppDbContext db) { ... }
        // ...
    }
}
```

Handlers are local static functions inside the `Register*Endpoints` method, declared after the route registrations. They take dependencies as parameters (DI is per-handler, not per-class).

`ConfigureApp.Configure` calls each `Register*Endpoints` extension after the middleware pipeline is set up.

## Routes are unprefixed

Routes do not use `/api/...`. They sit at the top level: `/categories`, `/products/all/`, `/users/login`, etc. Don't introduce an `/api` prefix without changing every existing route and the frontend's expectations (the Next.js middleware already strips `/api/` before forwarding).

## Return types: `IResult` + `TypedResults`

Handlers return `Task<IResult>` and use `TypedResults.Ok(...)`, `TypedResults.NotFound()`, `TypedResults.Created(uri, value)`, `TypedResults.BadRequest(message)`, etc. `TypedResults` over `Results` because:

- the return type is preserved at compile time (testable),
- response metadata is automatically attached for OpenAPI.

Don't mix the two styles in a single handler.

## DTOs over entities at the API boundary

Endpoints project entities into DTOs (`Data/Types/*DTO.cs`) before returning. Don't return raw `Product`, `User`, etc.; serialisation can leak fields (e.g. `PasswordHash`) and EF tracking annotations.

Use `.AsNoTracking()` for read-only queries and project directly into the DTO via `Select(...)` to avoid loading unused columns.

## Database

- DbContext: `Data/AppDbContext.cs`. All `DbSet`s + `OnModelCreating` configuration (table names, FKs, indexes, computed columns).
- Tables use lowercase plural names; entity classes use PascalCase singular.
- Cascading deletes are configured for parent-child aggregates (User → Orders/CartItems, Category → SubCategories, etc.).
- Indexes: `Product.SubCategoryId`, `SubCategory.CategoryId`, `Order.UserId`, `OrderItem.OrderId`, `CartItem.UserId`.
- Computed columns: `Category.slug`, `SubCategory.slug`, `Product.slug` (lower-replace title), `Product.sale_price` (price × (1 - sale/100)). Declared via `HasComputedColumnSql(..., stored: true)` so EF knows not to insert them.
- Connection string: parsed via `System.Uri` + `NpgsqlConnectionStringBuilder` in `ConfigureServices.ParsePostgresUrl`. Pass `postgres://...` URL form, not Npgsql key=value.

## Authentication and authorization

JWT bearer auth is wired in `ConfigureServices.AddJwtAuthentication` and the middleware (`UseAuthentication` + `UseAuthorization`) is enabled in `ConfigureApp.Configure`. Endpoints opt into auth at registration:

```csharp
group.MapPost("/", Create).RequireAuthorization("Admin");   // requires Admin role
group.MapGet("/", List).RequireAuthorization();              // any authenticated user
group.MapGet("/public", Read);                               // public, no auth
```

Named policies live in `ConfigureServices.AddAuthorizationPolicies`. Today only `"Admin"` is defined (`RequireRole("Admin")`). Add more there.

Token issuance is `JwtTokenHelper.GenerateToken(user)`, registered as a singleton; inject it as a parameter into login-style handlers. The helper uses the same `IOptions<JwtOptions>` that `AddJwtAuthentication` reads, so issuance and validation share configuration.

JWT config lives in the `Jwt` section (`Jwt:Issuer`, `Jwt:Audience`, `Jwt:SigningKey`, `Jwt:ExpirationMinutes`). In dev, defaults are in `appsettings.json`. In prod, set `Jwt__SigningKey` via `fly secrets set`.

## Adding auth to a new endpoint

1. Decide policy: public, authenticated, or `"Admin"`.
2. Append `.RequireAuthorization(...)` at registration time.
3. If you need the calling user, read claims from `HttpContext.User`:
   ```csharp
   static async Task<IResult> Handler(HttpContext ctx, AppDbContext db)
   {
       var userId = int.Parse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
       // ...
   }
   ```
   Don't take the user id as a path parameter for endpoints that should only operate on the caller's own data; use the JWT claim.

## Multi-aggregate writes wrap in a transaction

Any handler that mutates more than one aggregate must run inside `db.Database.BeginTransactionAsync()`. The canonical example is `OrdersEndpoints.PlaceOrder`, which:

1. Loads cart items and validates against client-supplied data
2. Loads the affected products and rejects if stock is insufficient
3. Decrements `Stock`, increments `Sold`
4. Inserts `Order` + `OrderItem` rows
5. Removes the user's `CartItem`s
6. Commits

If any step throws, the whole thing rolls back. Don't fall back to the multiple-`SaveChanges` pattern that existed before; it can leave the DB in an inconsistent state.

## Configuration

Don't touch `process.env`-style env vars directly. Read from `builder.Configuration.GetConnectionString(...)`, `builder.Configuration.GetSection(...)`, or `IOptions<T>` so dev/prod overrides via env vars (double-underscore for nested keys) work.

The `appsettings.json` connection string points at `localhost:15432` (the local Docker DB exposed to the host). Inside the backend container, the env var `ConnectionStrings__DATABASE_URL` overrides it to `db:5432` (compose network DNS). Keep both in sync if you change the local DB credentials.

CORS allowed origins come from `Cors:AllowedOrigins` (string array). Empty array = no cross-origin allowed.

## Migrations workflow

Migrations live in `Migrations/`. The baseline (`20260426151538_InitialCreate`) matches the prod schema (including computed columns). `__EFMigrationsHistory` on each environment has it marked applied; on a fresh local DB it gets inserted by `dotnet ef database update`, on a DB restored from prod dump it gets inserted manually (see root README, Path B).

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
2. Inside the extension, `var <feature> = app.MapGroup("/<feature>");` then register routes. Add `.RequireAuthorization(...)` per the auth model.
3. Add handlers as local static functions; project to DTOs.
4. Call `app.Register<Feature>Endpoints();` from `ConfigureApp.Configure`.
5. If new entities are involved: add the entity in `Data/Types/`, add the `DbSet` and `OnModelCreating` configuration in `AppDbContext`, then `dotnet ef migrations add Add<Feature>`.

## Things to avoid

- **Don't hardcode secrets or credentials.** Use `IOptions<T>` and Fly secrets.
- **Don't return entities directly** from endpoints; always project to a DTO.
- **Don't use `Results.Ok(...)`** when `TypedResults.Ok(...)` is available; you lose OpenAPI metadata.
- **Don't take `userId` as a path parameter** for endpoints that operate on the caller's own data. Read it from the JWT claim instead. Path-param userIds let any authenticated user act as anyone else.
- **Don't add a global `app.UseExceptionHandler`** without thinking through the error-response shape; the API currently has none, so any added one becomes the contract.
- **Don't share `Endpoints` registrations across features.** One feature, one file, one `MapGroup`.
- **Don't run multi-aggregate writes outside a transaction.**
