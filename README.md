# Next Net Shop Backend

The .NET 9 Minimal API for Next Net Shop. EF Core 9 against PostgreSQL, JWT issuance with BCrypt password hashing, NSwag-generated OpenAPI.

> Project conventions and gotchas live in [CLAUDE.md](CLAUDE.md). This file is the user-facing tour.

## Stack

- .NET 9 (Minimal API style: `MapGroup` + `MapGet`/`MapPost`/etc.)
- Entity Framework Core 9 with `Npgsql.EntityFrameworkCore.PostgreSQL`
- PostgreSQL 17 (local: Docker; prod: Fly Postgres `nextnetshop-db`)
- JWT auth via `Microsoft.AspNetCore.Authentication.JwtBearer` (currently configured but unenforced; see [CLAUDE.md](CLAUDE.md))
- BCrypt.Net for password hashing
- NSwag for OpenAPI generation and Swagger UI
- CSharpier for formatting

## Layout

```
net-backend/
├── Program.cs               # entry point
├── ConfigureServices.cs     # DI: CORS, OpenAPI, EF + Postgres URL parsing, JWT
├── ConfigureApp.cs          # middleware pipeline + endpoint registration
├── appsettings.json         # dev connection string (localhost:15432)
├── Categories/
│   ├── CategoriesEndpoints.cs
│   └── SubCategoriesEndpoints.cs
├── Products/ProductsEndpoints.cs
├── Users/
│   ├── UsersEndpoints.cs
│   └── JwtTokenHelper.cs
├── Cart/CartEndpoints.cs
├── Orders/OrdersEndpoints.cs
├── Data/
│   ├── AppDbContext.cs
│   └── Types/               # entities and DTOs
├── Migrations/              # EF Core migrations (baseline: InitialCreate)
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

## Endpoints

All routes are unprefixed (no `/api/`). Authorization middleware is currently disabled, so every endpoint is publicly accessible. JWT issuance still works at `/users/login`.

### Categories

```
GET    /categories/
GET    /categories/{id}
GET    /categories/{id}/image
POST   /categories/
PUT    /categories/{id}
DELETE /categories/{id}
```

### Subcategories

```
GET    /subcategories/
GET    /subcategories/{id}
GET    /subcategories/{id}/image
POST   /subcategories/
PUT    /subcategories/{id}
DELETE /subcategories/{id}
```

### Products

```
GET    /products/all/                                  # filterable: ?category, ?subcategory, ?priceMin, ?priceMax, ?sortBy, ?limit, ?page
GET    /products/top-deals
GET    /products/sales/
GET    /products/bestsellers/
GET    /products/search                                # ?query=
GET    /products/recommendations/{productId}
GET    /products/personal-recommendations/{userId}
GET    /products/id/{id}
GET    /products/slug/{slug}
GET    /products/{id}/image
POST   /products/
PUT    /products/id/{id}
DELETE /products/id/{id}
```

### Users

```
GET    /users/
GET    /users/id/{id}
POST   /users/register
POST   /users/admin/create
POST   /users/login          # returns JWT
```

### Cart

```
GET    /cart/user/{userId}
POST   /cart/
PUT    /cart/
DELETE /cart/item
DELETE /cart/user/{userId}
POST   /cart/sync/{userId}
```

### Orders

```
GET    /orders/user/{userId}
POST   /orders/user/{userId}
PUT    /orders/
```

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

DTOs are colocated in `Data/Types/` (suffixed `DTO.cs`). Endpoints project entities into DTOs before returning.

## Database migrations

Migrations live in `Migrations/`. The baseline is `InitialCreate`, generated against the schema dumped from prod. The `__EFMigrationsHistory` table on the local DB has `InitialCreate` marked as already applied (it was inserted manually after restoring the prod dump, so EF won't try to recreate the existing tables).

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

`ConfigureServices.cs` reads the database URL by environment:

- **Development**: `ConnectionStrings:DATABASE_URL` from configuration. Default value lives in `appsettings.json` (`postgres://...@localhost:15432/...`). Override via env var `ConnectionStrings__DATABASE_URL` (the compose file does this to point at `db:5432`).
- **Production**: bare `DATABASE_URL` env var (Fly secret).

The URL is parsed from `postgres://user:pass@host:port/db?sslmode=...` into Npgsql's `Host=...;Port=...;User Id=...;Password=...;Database=...;` format. In production, the `flycast` hostname is rewritten to `internal` for Fly's 6PN routing.

JWT issuer, audience, and signing key are currently hardcoded in `ConfigureServices.AddJwtAuthentication`. Move these to configuration before any non-demo deployment.

## Deployment (Fly.io)

```bash
fly deploy                  # build + push + release using net-backend/Dockerfile
fly logs
fly ssh console
fly status
```

The prod machine is configured with `min_machines_running = 0` and `auto_stop_machines = 'stop'` (see `fly.toml`), so it's suspended when idle and the first request after a quiet period takes a few seconds to wake.

## Troubleshooting

- **`Failed to connect to 127.0.0.1:15432`** when the backend runs in Docker: appsettings.json beat the env var. The expected env var name is `ConnectionStrings__DATABASE_URL` (double underscore). Restart the backend after fixing.
- **Migration fails because tables already exist**: the schema came from a dump but `__EFMigrationsHistory` wasn't updated. Insert a row marking the baseline migration as applied (see [CLAUDE.md](CLAUDE.md)).
- **Hot reload didn't pick up startup changes**: edits to DI/middleware require a full restart. `docker compose restart backend`.
