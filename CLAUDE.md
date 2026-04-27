# Backend conventions and gotchas

Stack-specific rules for `net-backend`. Project-wide conventions live in `../CLAUDE.md`.

## Architecture: modular monolith with DDD layers

Each bounded context lives under `Modules/<Feature>/` with its own DDD layers. Cross-cutting infrastructure stays at the project root.

```
net-backend/
├── Configuration/                # cross-cutting service registration
│   ├── KestrelConfiguration.cs
│   ├── CorsConfiguration.cs
│   ├── OpenApiConfiguration.cs
│   ├── DatabaseConfiguration.cs
│   ├── AuthConfiguration.cs
│   └── ExceptionHandlingConfiguration.cs
├── Common/                       # cross-cutting code
│   ├── Auth/UserContextExtensions.cs
│   └── Exceptions/{AppException, NotFoundException, ValidationException,
│                   ConflictException, ForbiddenException, UnauthorizedException,
│                   GlobalExceptionHandler}.cs
├── Data/                         # EF model
│   ├── AppDbContext.cs
│   └── Types/                    # entities and supporting types
├── Migrations/                   # EF Core migrations
├── Modules/<Feature>/            # bounded contexts
│   ├── Domain/                   # interfaces + entity-aware logic
│   ├── Infrastructure/           # EF implementations
│   ├── Application/
│   │   ├── Queries/              # read use cases (one file each)
│   │   └── Commands/             # write use cases (one file each)
│   ├── Contracts/                # DTOs and Request types
│   ├── <Feature>Controller.cs    # thin HTTP adapter
│   └── <Feature>Module.cs        # per-feature DI registration
├── ConfigureApp.cs               # request pipeline
├── ConfigureServices.cs          # orchestrates Configuration + Modules
└── Program.cs                    # 10-line entry point
```

A new feature = a new `Modules/<Feature>/` folder, plus one line in `ConfigureServices.AddServices` to register its module. Nothing else changes.

## The DDD layers (per module)

| Layer | Job | Knows about |
|---|---|---|
| Domain | Entity logic + repository interface + domain services | Pure types and other domain interfaces |
| Infrastructure | EF Core implementation of repository interfaces | Domain + EF + AppDbContext |
| Application | Use cases: one Handler per query / command | Domain interfaces only |
| Contracts | DTOs and validated request types | Plain types + DataAnnotations |
| Controller | HTTP adapter: validate + read claims + call handler + project | All of the above |

Dependencies always point **inward**: Controller → Application → Domain. Infrastructure plugs in via DI as the implementation of a Domain interface.

## Routing pattern

All routes are unprefixed (no `/api/...`). Controllers use lowercase explicit routes to preserve the public API the frontend depends on:

```csharp
[ApiController]
[Route("categories")]               // explicit lowercase, not [controller]
public class CategoriesController(...) : ControllerBase
{
    [HttpGet("")]
    public async Task<ActionResult<List<CategoryDto>>> List(...) { ... }

    [HttpPost("")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryRequest req, ...) { ... }
}
```

Tokens to know:
- `[ApiController]` enables auto 400 ProblemDetails on model-binding failure, automatic `[FromBody]`, and other niceties.
- `[Route("xxx")]` sets the controller's base path. Methods add segments via `[HttpGet("...")]`.
- Route constraints (`{id:int}`, `{slug}`) disambiguate when two methods share a base path.

## Return types: `IResult` is gone, `IActionResult` rules

Action methods return:
- `ActionResult<TDto>` for success-with-body endpoints. The `T` flows into OpenAPI as the success shape; helpers on `ControllerBase` produce HTTP responses (`Ok`, `CreatedAtAction`, `NotFound`, `BadRequest`, `NoContent`, `File`, etc.).
- `IActionResult` for actions where the response shape varies (e.g. `File(...)` for image bytes, `NoContent()` after a delete).

Don't use `Results.*` / `TypedResults.*` (those are Minimal API only).

## Authorization model

Middleware is enabled in `ConfigureApp.Configure` (`UseAuthentication` + `UseAuthorization`). Endpoints opt in at attribute time:

```csharp
[Authorize]                         // any authenticated user
[Authorize(Policy = "Admin")]       // RequireRole("Admin")
// no attribute = public
```

Policies live in `Configuration/AuthConfiguration.AddAuthorizationPolicies`. Today: `"Admin"`. Add new ones there.

JWT issuance + validation share `IOptions<JwtOptions>`. Issuance is `Modules/Users/Domain/JwtTokenHelper`; validation is registered by `AuthConfiguration.AddJwtAuthentication`. Both read from the `Jwt` config section.

## Reading the user from the JWT claim

Never accept a `userId` in a path or body parameter for endpoints that operate on the caller's own data. Use the extension on `ClaimsPrincipal`:

```csharp
using net_backend.Common.Auth;

[HttpGet("")]
[Authorize]
public async Task<ActionResult<...>> Mine(CancellationToken ct)
{
    var userId = User.GetRequiredUserId();
    return Ok(await handler.ExecuteAsync(userId, ct));
}
```

`GetRequiredUserId` reads the `NameIdentifier` claim and parses it to `int`. Throws `ForbiddenException` (which the global handler turns into 403) if the claim is missing or malformed; should never trip behind `[Authorize]`, but the throw makes the contract explicit.

## Typed exception hierarchy + ProblemDetails

Domain code throws typed exceptions; the global handler (`Common/Exceptions/GlobalExceptionHandler`) catches everything and emits RFC 7807 `application/problem+json`. Stack traces are dev-only.

| Exception | Status | When |
|---|---|---|
| `NotFoundException` | 404 | Resource the client requested doesn't exist |
| `ValidationException` | 400 | Invalid input that the request DTO annotations didn't catch (custom rules, cross-field) |
| `ConflictException` | 409 | Duplicate row, version conflict, state conflict |
| `ForbiddenException` | 403 | Authenticated but not allowed (e.g. acting on someone else's resource) |
| `UnauthorizedException` | 401 | Authentication failure (bad credentials, expired token) |

Each carries an `ErrorCode` (e.g. `CATEGORY_NOT_FOUND`, `EMAIL_TAKEN`, `INSUFFICIENT_STOCK`) for client-side switching, surfaced under the `errorCode` extension in the ProblemDetails response.

Don't catch and rethrow in handlers; let the exception bubble. The global handler is the only place that formats error responses.

## Repository pattern

Each module declares an interface in `Domain/I<Aggregate>Repository.cs`. The EF implementation lives in `Infrastructure/Ef<Aggregate>Repository.cs`. Handlers depend on the interface only.

Read methods that don't need a tracked entity should:
- Use `.AsNoTracking()`
- Project to the DTO via `Select(...)` or a shared `Expression<Func<Entity, Dto>> Projection` (see `Modules/Products/Contracts/ProductDto.Projection`)
- `.Include(...)` only what's actually serialised; never load `byte[]` columns when the DTO doesn't need them

Mutation methods load the entity (no `.AsNoTracking()`), mutate, and call `SaveChangesAsync`. Aggregates that need a transaction wrap with `db.Database.BeginTransactionAsync` (see `OrderPlacement.PlaceFromCartAsync`).

## Domain services

Multi-aggregate logic lives in a class named for the **business action**: never `FooService` / `FooManager` / `FooHelper`. Live examples:

- `Modules/Orders/Domain/OrderPlacement`: places an order from a cart (validates client cart matches server, decrements stock with concurrency, creates order + items, clears cart, all in one transaction).
- `Modules/Users/Domain/Authentication`: verifies email + password (constant-time bcrypt; no enumeration leak).

When a handler does more than one thing across multiple aggregates, extract to a domain service. Single-aggregate logic can stay in the handler.

## DTOs at the API boundary

Endpoints project entities into DTOs in `Contracts/`. Don't return raw entities; serialisation can leak fields (e.g. `PasswordHash`) and EF tracking annotations.

Each `*Dto.cs` exposes a `FromEntity` static factory and, where used in queries, an `Expression<Func<Entity, Dto>> Projection` for SQL-level projection. Pattern (from `ProductDto`):

```csharp
public record ProductDto(int Id, string Title, ...)
{
    public static Expression<Func<Product, ProductDto>> Projection =>
        p => new ProductDto(p.Id, p.Title, ...);
    public static ProductDto FromEntity(Product p) => new(p.Id, p.Title, ...);
}
```

The `Projection` form is what your read-side queries should use to translate to SQL `SELECT` clauses.

## Adding a new feature (recipe)

1. **Pick a name** for the bounded context. Plural feature folder: `Modules/<Feature>/`.
2. **Entity**: add to `Data/Types/<Entity>.cs` with `[Column("...")]` annotations matching the schema.
3. **`Modules/<Feature>/Domain/I<Aggregate>Repository.cs`**: interface for persistence operations.
4. **`Modules/<Feature>/Infrastructure/Ef<Aggregate>Repository.cs`**: EF implementation.
5. **`Modules/<Feature>/Contracts/`**:
   - `<Aggregate>Dto.cs` with `FromEntity` and `Projection` if used by queries.
   - `<Verb><Aggregate>Request.cs` per write endpoint, with DataAnnotations.
6. **`Modules/<Feature>/Application/Queries/`**: one Handler per read use case.
7. **`Modules/<Feature>/Application/Commands/`**: one Handler per write use case.
8. **`Modules/<Feature>/<Feature>Controller.cs`**: thin `[ApiController]` with method-level `[Authorize]`.
9. **`Modules/<Feature>/<Feature>Module.cs`**: registers the repository, handlers, and any domain service in DI.
10. **`ConfigureServices.AddServices`**: append `builder.Add<Feature>Feature();`.
11. **Migration**: run `dotnet ef migrations add Add<Feature>` from `net-backend/`, review the generated DDL, commit it.
12. **DbSet**: register the entity in `AppDbContext` with any indexes/relations.

Order matters when there are cross-aggregate dependencies: declare the parent's repository interface before depending on it.

## Multi-aggregate writes wrap in a transaction

Any handler that mutates more than one aggregate must run inside `db.Database.BeginTransactionAsync()`. Canonical example: `OrderPlacement.PlaceFromCartAsync` decrements products, inserts orders + order items, and removes cart items in one transaction.

If any step throws, the whole thing rolls back. Don't fall back to multiple `SaveChanges` calls without a wrapping transaction.

## Configuration

Read from `builder.Configuration.GetSection(...)` or `IOptions<T>`. Don't access `Environment.GetEnvironmentVariable` outside the config layer. Env vars override appsettings via the double-underscore convention (`Cors__AllowedOrigins__0`, `Jwt__SigningKey`, `ConnectionStrings__DATABASE_URL`).

`appsettings.json` holds dev defaults. Production secrets (specifically `Jwt__SigningKey`) are set via `fly secrets set` and never live in source.

## Migrations workflow

Migrations live in `Migrations/`. The baseline is `20260426151538_InitialCreate` (matches the prod schema, including computed columns).

```bash
cd net-backend
dotnet ef migrations add <PascalCaseName>
# Review the generated Migrations/*.cs file BEFORE applying
dotnet ef database update
```

If a generated migration tries to recreate an existing table because the model and the live schema disagree, don't apply it. Reconcile the model to reality, or `dotnet ef migrations remove` and write the migration by hand.

## Things to avoid

- **Don't put feature code outside `Modules/<Feature>/`.** Cross-cutting concerns go in `Common/` or `Configuration/`; data goes in `Data/`. Everything else is a domain feature.
- **Don't take `userId` as a path or body parameter** for endpoints that operate on the caller's own data. `User.GetRequiredUserId()` from the JWT claim is the answer.
- **Don't return entities directly** from controllers; always project to a DTO.
- **Don't share routes between controllers.** One controller per feature; one `[Route("xxx")]` per controller.
- **Don't put business logic in controllers.** Validate input, call handler, return DTO. That's it.
- **Don't catch + rethrow in handlers.** Let typed exceptions bubble; the global handler formats them.
- **Don't run multi-aggregate writes outside a transaction.**
- **Don't use `Results.*` / `TypedResults.*`** in controllers: those are Minimal API. Use `Ok()`, `NotFound()`, `CreatedAtAction()`, etc. from `ControllerBase`.
- **Don't hardcode secrets**, even temporarily. `appsettings.json` placeholders for dev are fine; prod uses Fly secrets.
- **Don't add a generic `<Feature>Service` class.** Either it's a Handler (one use case) or a domain service named for the business action.
