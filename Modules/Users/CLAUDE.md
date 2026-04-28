# Users module

User registration, login, JWT issuance, and admin user management. The only module that owns auth-token machinery; everything else just consumes JWTs.

For DDD pattern rules see the parent [`net-backend/CLAUDE.md`](../../CLAUDE.md). For the cross-module landscape see [`Modules/CLAUDE.md`](../CLAUDE.md).

## Files at this level

| File | Purpose |
|---|---|
| `UsersController.cs` | HTTP adapter; mixed auth model per route |
| `UsersModule.cs` | DI registration (registers `Authentication` and `JwtTokenHelper`) |
| `JwtTokenHelper.cs` | Builds + signs the JWT on successful login (uses `IOptions<JwtOptions>`) |
| `JwtOptions.cs` | Strongly-typed config bound from the `Jwt` config section |
| `Domain/IUserRepository.cs` | Persistence contract |
| `Domain/Authentication.cs` | Domain service: verify email + password with constant-time bcrypt |
| `Infrastructure/EfUserRepository.cs` | EF implementation |
| `Application/Queries/{ListUsersHandler, GetUserByIdHandler}.cs` | Read use cases |
| `Application/Commands/{RegisterUserHandler, CreateAdminHandler, LoginHandler}.cs` | Write use cases |
| `Contracts/{UserDto, RegisterUserRequest, LoginRequest, LoginResponse}.cs` | API surface |

## Routes

| Method | Path | Auth | Handler | Notes |
|---|---|---|---|---|
| GET | `/users` | `[Authorize(Policy = "Admin")]` | `ListUsersHandler` | Admin-only directory |
| GET | `/users/{id:int}` | `[Authorize]` | `GetUserByIdHandler` | Any authenticated user |
| POST | `/users/register` | public | `RegisterUserHandler` | Self-service signup |
| POST | `/users/admin` | `[Authorize(Policy = "Admin")]` | `CreateAdminHandler` | Admin creates another admin |
| POST | `/users/login` | public | `LoginHandler` | Returns JWT in `LoginResponse` |

## The Authentication domain service

`Authentication.AuthenticateAsync(email, password)` is the only place that verifies credentials. It does two things that matter:

1. **Constant-time response** regardless of whether the email exists. If `FindByEmailAsync` returns `null`, the service still runs `BCrypt.Verify(password, DummyHash)` so the request takes the same time as a real verify. Without this, an attacker could enumerate registered emails by timing the response.

2. **Same exception, same error code, same message** for "no such email" and "wrong password" — both throw `UnauthorizedException("Invalid email or password.", "INVALID_CREDENTIALS")`. The client can't distinguish the two cases. Don't change this without understanding why it's that way.

`LoginHandler` is the only consumer; it calls `Authenticate`, then hands the user to `JwtTokenHelper.IssueToken` which returns the signed JWT.

## JWT specifics

- **Signing key**: `JwtOptions.SigningKey`, set in dev via `appsettings.json` (committed placeholder, fine to commit) and in prod via `fly secrets set Jwt__SigningKey=...`.
- **Lifetime**: configured in `JwtOptions.AccessTokenLifetimeMinutes`. No refresh tokens today — when an access token expires, the user re-logs in.
- **Claims**: `NameIdentifier` (the user's int id, parsed by `User.GetRequiredUserId()`), `Email`, `Role` (used by the `"Admin"` policy via `RequireRole`).
- **Token validation**: configured by `Configuration/AuthConfiguration.AddJwtAuthentication`; reads the same `JwtOptions`. Issuance and validation always agree because both bind the same options class.

## Module-specific notes

- **Password hashing**: bcrypt via `BCrypt.Net.BCrypt.HashPassword`. Cost factor is the library default (10–12). Hashes are stored in `User.PasswordHash`.
- **`UserDto` never includes `PasswordHash`**. Don't add it to the projection. Don't return raw `User` entities.
- **`CreateAdmin` is admin-only**. There is no public path to becoming an admin. To bootstrap the first admin, seed manually via SQL or via a one-off CLI tool.
- **Email is the username**. Unique index on `Email`. Lowercase comparisons happen at the repository level so the user's casing on signup doesn't lock them out at login.
- **No email verification, no password reset** today. Both are deferred work; the `Forgot password` UI in the frontend has no backend wired up.
