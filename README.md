# Next Net Shop Backend

The .NET 9 modular monolith backend for Next Net Shop. EF Core 9 + PostgreSQL, JWT bearer auth, MVC controllers grouped by bounded context under `Modules/<Feature>/`, with full DDD layers (Domain / Infrastructure / Application / Contracts) per feature.

> Project conventions and gotchas live in [CLAUDE.md](CLAUDE.md). This file is the user-facing tour.

## Stack

- .NET 9 with ASP.NET Core MVC controllers
- Entity Framework Core 9 with `Npgsql.EntityFrameworkCore.PostgreSQL`
- PostgreSQL 17 (local: Docker; prod: Fly Postgres `nextnetshop-db`)
- JWT bearer auth via `Microsoft.AspNetCore.Authentication.JwtBearer`, fully config-driven (`JwtOptions`)
- BCrypt.Net for password hashing
- NSwag for OpenAPI / Swagger UI
- CSharpier for formatting

## Layout

```
net-backend/
├── Configuration/                # cross-cutting service registration
│   ├── KestrelConfiguration.cs   # port binding
│   ├── CorsConfiguration.cs      # origin allowlist
│   ├── OpenApiConfiguration.cs   # Swagger
│   ├── DatabaseConfiguration.cs  # EF + Postgres URL parser
│   ├── AuthConfiguration.cs      # JwtOptions + Authorization policies
│   └── ExceptionHandlingConfiguration.cs
├── Common/
│   ├── Auth/UserContextExtensions.cs   # ClaimsPrincipal.GetRequiredUserId()
│   └── Exceptions/                     # typed AppException hierarchy + GlobalExceptionHandler
├── Data/
│   ├── AppDbContext.cs
│   └── Types/                    # entity classes
├── Migrations/                   # EF Core migrations
├── Modules/                      # bounded contexts
│   ├── Categories/
│   ├── SubCategories/
│   ├── Cart/
│   ├── Orders/
│   ├── Users/
│   └── Products/
├── ConfigureApp.cs               # request pipeline
├── ConfigureServices.cs          # composes Configuration + Modules
├── Program.cs
├── Dockerfile                    # prod multi-stage image
├── Dockerfile.dev                # dev image (SDK + dotnet watch)
├── seed-data.sql                 # categories/subcategories/products fixture
└── fly.toml                      # Fly.io app config (nextnetshop-backend)
```

Each module has the same shape:

```
Modules/<Feature>/
├── Domain/
│   ├── I<Aggregate>Repository.cs    # contract
│   ├── <Aggregate>.cs               # if entity has rich logic (factories, state transitions)
│   └── <DomainService>.cs           # named for the business action; e.g. OrderPlacement, Authentication
├── Infrastructure/
│   └── Ef<Aggregate>Repository.cs   # EF Core impl
├── Application/
│   ├── Queries/
│   │   └── <Verb><Aggregate>Handler.cs
│   └── Commands/
│       └── <Verb><Aggregate>Handler.cs
├── Contracts/
│   ├── <Aggregate>Dto.cs            # output shape, with FromEntity + Projection
│   └── <Verb><Aggregate>Request.cs  # validated input shape
├── <Feature>Controller.cs           # thin [ApiController]
└── <Feature>Module.cs               # per-feature DI registration extension
```

## Quick start

The recommended local workflow runs the backend in Docker via the parent repo's `docker-compose.yml`. From the parent repo root:

```bash
docker compose up -d        # Postgres + backend with hot reload
docker compose logs -f backend
```

URLs:
- API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger

Host-side dev (without Docker):

```bash
dotnet restore
dotnet watch run            # auto-rebuild on file changes
# expects Postgres at localhost:15432 (run docker compose up -d db separately)
```

## Auth model

JWT bearer tokens are issued by `POST /users/login`. Authentication + Authorization middleware are enabled in `ConfigureApp.Configure`. Endpoints opt into auth via attributes:

- `[Authorize(Policy = "Admin")]`: requires the Admin role claim. Catalog mutations and admin-only user endpoints.
- `[Authorize]`: any authenticated user. Cart, orders, personal recommendations.
- No attribute: public. Catalog reads, login, register.

The `"Admin"` policy lives in `Configuration/AuthConfiguration.AddAuthorizationPolicies`. Add new named policies there.

User identity comes from the JWT `NameIdentifier` claim via `User.GetRequiredUserId()` in `Common/Auth/UserContextExtensions`. Endpoints that operate on the caller's own data never accept a `userId` in the path or body.

## Endpoints

All routes unprefixed (no `/api/`). Auth column: P = public, A = authenticated user, ★ = `"Admin"` policy.

### Categories

| Method | Path | Auth |
|---|---|---|
| GET | `/categories` | P |
| GET | `/categories/{id:int}` | P |
| GET | `/categories/{id:int}/image` | P |
| POST | `/categories` | ★ |
| PUT | `/categories/{id:int}` | ★ |
| DELETE | `/categories/{id:int}` | ★ |

### Subcategories

| Method | Path | Auth |
|---|---|---|
| GET | `/subcategories` | P |
| GET | `/subcategories/{id:int}` | P |
| GET | `/subcategories/{id:int}/image` | P |
| POST | `/subcategories` | ★ |
| PUT | `/subcategories/{id:int}` | ★ |
| DELETE | `/subcategories/{id:int}` | ★ |

Cross-aggregate validation: Create/Update verify the parent `CategoryId` exists via `ICategoryRepository`.

### Products

| Method | Path | Auth |
|---|---|---|
| GET | `/products/all?category&subcategory&priceMin&priceMax&sortBy&limit&page` | P |
| GET | `/products/sales?...` | P |
| GET | `/products/bestsellers?...` | P |
| GET | `/products/top-deals` | P |
| GET | `/products/search?query=` | P |
| GET | `/products/recommendations/{productId:int}` | P |
| GET | `/products/personal-recommendations` | A |
| GET | `/products/id/{id:int}` | P |
| GET | `/products/slug/{slug}` | P |
| GET | `/products/{id:int}/image` | P |
| POST | `/products` | ★ |
| PUT | `/products/id/{id:int}` | ★ |
| DELETE | `/products/id/{id:int}` | ★ |

### Users

| Method | Path | Auth |
|---|---|---|
| GET | `/users` | ★ |
| GET | `/users/{id:int}` | A |
| POST | `/users/register` | P |
| POST | `/users/admin` | ★ |
| POST | `/users/login` (returns `{user, token}`) | P |

Login throws `UnauthorizedException` (→ 401) on both unknown email and bad password; constant-time bcrypt verify means the response timing doesn't leak whether an email exists. Register/CreateAdmin throw `ConflictException` (→ 409) on duplicate email.

### Cart (all `[Authorize]`)

| Method | Path |
|---|---|
| GET | `/cart` |
| POST | `/cart/items` |
| PUT | `/cart/items` |
| DELETE | `/cart/items/{productId:int}` |
| DELETE | `/cart` |
| POST | `/cart/sync` |

User from JWT claim. `POST /cart/items` increments quantity if the product is already in the cart; `PUT /cart/items` sets an absolute quantity. `POST /cart/sync` atomically replaces the user's cart with the supplied items (used at sign-in to merge a guest cart).

### Orders (all `[Authorize]`)

| Method | Path | Extra auth |
|---|---|---|
| GET | `/orders` | (any user) |
| POST | `/orders` | (any user) |
| PUT | `/orders/{id:int}/status` | ★ |

`POST /orders` invokes the `OrderPlacement` domain service: validates the client's cart matches the server's, decrements stock per product (rolls back on insufficient stock), creates the order with line items, clears the cart, all in one `BeginTransactionAsync`. Status updates are admin-only and validate against a whitelist of allowed values.

## Domain model

Entities in `Data/Types/`. Tables use lowercase plural names (set in `AppDbContext.OnModelCreating`).

| Entity | Table | Owns | Belongs to |
|---|---|---|---|
| `User` | `users` | `Orders`, `CartItems` | |
| `Category` | `categories` | `SubCategories` | |
| `SubCategory` | `subcategories` | `Products` | `Category` |
| `Product` | `products` | | `SubCategory` |
| `Order` | `orders` | `OrderItems` | `User` |
| `OrderItem` | `orderitems` | | `Order`, `Product` |
| `CartItem` | `cartitems` | | `User`, `Product` |

Computed columns are declared via `HasComputedColumnSql(stored: true)`:
- `Category.slug`, `SubCategory.slug`, `Product.slug` derive from `lower(replace(title, ' ', '-'))`
- `Product.sale_price` derives from `price × (1 - sale/100)`

DTOs are colocated in each module's `Contracts/`.

## Database migrations

Migrations live in `Migrations/`. The baseline is `20260426151538_InitialCreate` (matches the prod schema, including computed columns).

```bash
cd net-backend
dotnet ef migrations add <PascalCaseName>
dotnet ef database update           # apply pending migrations to local DB
dotnet ef migrations list           # see what's applied
dotnet ef migrations remove         # undo last (only if not yet applied)
```

Install once: `dotnet tool install --global dotnet-ef`.

When restoring from prod dump (no `__EFMigrationsHistory` table in prod), insert the baseline manually so EF doesn't try to recreate existing tables. See the root README's first-time-setup, Path B.

## Configuration

Everything reads via `builder.Configuration` or `IOptions<T>`. Env vars override appsettings via the double-underscore convention.

| Section | Keys | Notes |
|---|---|---|
| `ConnectionStrings:DATABASE_URL` | postgres URL | dev: appsettings.json + override `ConnectionStrings__DATABASE_URL`. Prod: bare `DATABASE_URL` env var (Fly secret). |
| `Cors:AllowedOrigins` | string array | empty → locked down |
| `Jwt:Issuer`, `Jwt:Audience` | strings | token claims |
| `Jwt:SigningKey` | string | HMAC-SHA256 key. **Set via `fly secrets set Jwt__SigningKey=$(openssl rand -hex 32)` in prod.** |
| `Jwt:ExpirationMinutes` | int | default 60 |

Connection-string parsing uses `System.Uri` + `NpgsqlConnectionStringBuilder` so URL-encoded passwords, missing ports, and the `postgresql://` scheme variant all work. In production, `flycast` hostnames rewrite to `internal` for Fly's 6PN routing.

## Deployment (Fly.io)

```bash
fly deploy
fly logs
fly status
fly ssh console -a nextnetshop-backend -C 'printenv DATABASE_URL'
```

Before the first deploy of a fresh app:

```bash
fly secrets set Jwt__SigningKey=$(openssl rand -hex 32) -a nextnetshop-backend
fly secrets set Cors__AllowedOrigins__0=https://your-frontend.vercel.app -a nextnetshop-backend
```

The prod machine has `min_machines_running = 0` and `auto_stop_machines = 'stop'` (see `fly.toml`), so it suspends when idle. The first request after a quiet period takes a few seconds to wake.

## Troubleshooting

- **`Failed to connect to 127.0.0.1:15432`** when the backend runs in Docker: the env var `ConnectionStrings__DATABASE_URL` should override `appsettings.json`. Restart the backend after fixing.
- **`Jwt:SigningKey is not configured`** at startup: missing `Jwt__SigningKey`. Set it (see Configuration above).
- **401 from a previously-public endpoint**: the auth middleware now enforces `[Authorize]`. Either send a bearer token or remove the attribute at the controller method.
- **Migration fails because tables already exist**: schema came from a dump but `__EFMigrationsHistory` wasn't seeded. Insert a row marking the baseline migration as applied (see [CLAUDE.md](CLAUDE.md) and root README Path B).
- **Hot reload didn't pick up startup changes**: edits to DI/middleware require a full restart. `docker compose restart backend`.
- **`Cannot write DateTime with Kind=Local to PostgreSQL`**: a `DateTime` field is using `DateTime.Now` instead of `DateTime.UtcNow`. Npgsql 8+ refuses Local kind for `timestamptz` columns. Fix at the entity default.
