# Next Net Shop Backend

The .NET 9 Minimal API for Next Net Shop. EF Core 9 against PostgreSQL, JWT bearer auth with BCrypt password hashing, NSwag-generated OpenAPI.

> Project conventions and gotchas live in [CLAUDE.md](CLAUDE.md). This file is the user-facing tour.

## Stack

- .NET 9 (Minimal API style: `MapGroup` + `MapGet`/`MapPost`/etc.)
- Entity Framework Core 9 with `Npgsql.EntityFrameworkCore.PostgreSQL`
- PostgreSQL 17 (local: Docker; prod: Fly Postgres `nextnetshop-db`)
- JWT auth via `Microsoft.AspNetCore.Authentication.JwtBearer`, configuration-driven (`JwtOptions`)
- BCrypt.Net for password hashing
- NSwag for OpenAPI generation and Swagger UI
- CSharpier for formatting

## Layout

```
net-backend/
├── Program.cs               # entry point
├── ConfigureServices.cs     # DI: CORS, OpenAPI, EF + JWT auth + Authorization policies
├── ConfigureApp.cs          # middleware pipeline + endpoint registration
├── appsettings.json         # dev defaults: DB URL, Cors, Jwt
├── Categories/
│   ├── CategoriesEndpoints.cs
│   └── SubCategoriesEndpoints.cs
├── Products/ProductsEndpoints.cs
├── Users/
│   ├── UsersEndpoints.cs
│   ├── JwtOptions.cs        # strongly typed Jwt config
│   └── JwtTokenHelper.cs    # DI-injected, uses IOptions<JwtOptions>
├── Cart/CartEndpoints.cs
├── Orders/OrdersEndpoints.cs
├── Data/
│   ├── AppDbContext.cs
│   └── Types/               # entities and DTOs
├── Migrations/              # EF Core migrations (baseline: 20260426151538_InitialCreate)
├── seed-data.sql            # categories/subcategories/products fixture
├── Dockerfile               # prod image (multi-stage, runtime only)
├── Dockerfile.dev           # dev image (SDK + dotnet watch)
└── fly.toml                 # Fly.io app config (nextnetshop-backend)
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

To run on the host instead (without Docker):

```bash
dotnet restore
dotnet watch run            # auto-rebuild on file changes
# expects Postgres at localhost:15432 (run docker compose up -d db separately)
```

## Auth model

JWT bearer tokens are issued by `POST /users/login`. The middleware (`UseAuthentication` + `UseAuthorization`) is enabled in `ConfigureApp.cs`. Endpoints opt into auth at registration time:

- `.RequireAuthorization("Admin")`: requires the Admin role claim. Used for catalog mutations and the admin-only user endpoints.
- `.RequireAuthorization()`: requires any authenticated user. Used for cart and orders.
- No call: public endpoint. Catalog reads, login, register.

The "Admin" policy is defined in `ConfigureServices.AddAuthorizationPolicies`. Add new named policies there.

## Endpoints

All routes are unprefixed (no `/api/`). Auth column: P = public, A = authenticated user, ★ = "Admin" policy.

### Categories

| Method | Path | Auth |
|---|---|---|
| GET | `/categories/` | P |
| GET | `/categories/{id}` | P |
| GET | `/categories/{id}/image` | P |
| POST | `/categories/` | ★ |
| PUT | `/categories/{id}` | ★ |
| DELETE | `/categories/{id}` | ★ |

### Subcategories

| Method | Path | Auth |
|---|---|---|
| GET | `/subcategories/` | P |
| GET | `/subcategories/{id}` | P |
| GET | `/subcategories/{id}/image` | P |
| POST | `/subcategories/` | ★ |
| PUT | `/subcategories/{id}` | ★ |
| DELETE | `/subcategories/{id}` | ★ |

### Products

| Method | Path | Auth |
|---|---|---|
| GET | `/products/all/?category&subcategory&priceMin&priceMax&sortBy&limit&page` | P |
| GET | `/products/top-deals` | P |
| GET | `/products/sales/` | P |
| GET | `/products/bestsellers/` | P |
| GET | `/products/search?query=` | P |
| GET | `/products/recommendations/{productId}` | P |
| GET | `/products/personal-recommendations/{userId}` | A |
| GET | `/products/id/{id}` | P |
| GET | `/products/slug/{slug}` | P |
| GET | `/products/{id}/image` | P |
| POST | `/products/` | ★ |
| PUT | `/products/id/{id}` | ★ |
| DELETE | `/products/id/{id}` | ★ |

### Users

| Method | Path | Auth |
|---|---|---|
| GET | `/users/` | ★ |
| GET | `/users/id/{id}` | A |
| POST | `/users/register` | P |
| POST | `/users/admin/create` | ★ |
| POST | `/users/login` (returns JWT) | P |

### Cart (all require auth)

| Method | Path |
|---|---|
| GET | `/cart/user/{userId}` |
| POST | `/cart/` |
| PUT | `/cart/` |
| DELETE | `/cart/item` |
| DELETE | `/cart/user/{userId}` |
| POST | `/cart/sync/{userId}` |

### Orders

| Method | Path | Auth |
|---|---|---|
| GET | `/orders/user/{userId}` | A |
| POST | `/orders/user/{userId}` | A |
| PUT | `/orders/` | ★ |

`POST /orders/user/{userId}` runs inside a single transaction: cart validation, stock decrement on each `Product`, order + items insert, cart removal. If any step fails, the whole thing rolls back.

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

`Product.slug` and `Product.sale_price` are Postgres `GENERATED ALWAYS AS ... STORED` columns; the EF model declares them via `HasComputedColumnSql` so migrations and inserts stay in sync. Same for `Category.slug` and `SubCategory.slug`.

DTOs are colocated in `Data/Types/` (suffixed `DTO.cs`). Endpoints project entities into DTOs before returning.

## Database migrations

Migrations live in `Migrations/`. The baseline is `20260426151538_InitialCreate`. The `__EFMigrationsHistory` table on the local DB has it marked as already applied (it was inserted manually after restoring the prod dump, so EF won't try to recreate the existing tables).

```bash
# Create a new migration (run from net-backend on the host with dotnet-ef installed)
dotnet ef migrations add <Name>

# Apply pending migrations to the local DB
dotnet ef database update

# List migrations and their applied status
dotnet ef migrations list

# Undo the most recent migration (only if not yet applied)
dotnet ef migrations remove
```

Install `dotnet-ef` once: `dotnet tool install --global dotnet-ef`.

## Configuration

All configuration is read from `appsettings.json` + env var overrides (double-underscore for nested keys, e.g. `Jwt__SigningKey`).

| Section | Keys | Notes |
|---|---|---|
| `ConnectionStrings:DATABASE_URL` | `postgres://...` URL | Dev: appsettings.json. Prod: bare `DATABASE_URL` env var (Fly secret). Compose overrides via `ConnectionStrings__DATABASE_URL`. |
| `Cors:AllowedOrigins` | string array | Frontend origins. Empty array = locked down. |
| `Jwt:Issuer` | string | Token `iss` claim. |
| `Jwt:Audience` | string | Token `aud` claim. |
| `Jwt:SigningKey` | string | HMAC-SHA256 key. **Set via Fly secret in prod**: `fly secrets set Jwt__SigningKey=$(openssl rand -hex 32)`. |
| `Jwt:ExpirationMinutes` | int | Default 60. |

The connection string parser uses `System.Uri` + `NpgsqlConnectionStringBuilder`, so URL-encoded passwords, `postgresql://` schemes, and missing ports are all handled. In production, `flycast` hostnames are rewritten to `internal` for Fly's 6PN routing.

## Deployment (Fly.io)

```bash
fly deploy                  # build + push + release using net-backend/Dockerfile
fly logs
fly ssh console
fly status
```

Before the first deploy of a fresh app, set the secrets:

```bash
fly secrets set Jwt__SigningKey=$(openssl rand -hex 32) -a nextnetshop-backend
fly secrets set Cors__AllowedOrigins__0=https://your-frontend.vercel.app -a nextnetshop-backend
```

The prod machine has `min_machines_running = 0` and `auto_stop_machines = 'stop'` (see `fly.toml`), so it suspends when idle. The first request after a quiet period takes a few seconds to wake the machine.

## Troubleshooting

- **`Failed to connect to 127.0.0.1:15432`** when the backend runs in Docker: appsettings.json beat the env var. The expected env var name is `ConnectionStrings__DATABASE_URL` (double underscore). Restart the backend after fixing.
- **`Jwt:SigningKey is not configured`** at startup: missing `Jwt__SigningKey` env var or `Jwt:SigningKey` in appsettings. Set it (see Configuration above).
- **401 from a previously-public endpoint**: the auth middleware now enforces `RequireAuthorization()`. Either log in via `/users/login` and send the bearer token, or remove the auth requirement at the endpoint registration site.
- **Migration fails because tables already exist**: the schema came from a dump but `__EFMigrationsHistory` wasn't updated. Insert a row marking the baseline migration as applied (see [CLAUDE.md](CLAUDE.md)).
- **Hot reload didn't pick up startup changes**: edits to DI/middleware require a full restart. `docker compose restart backend`.
