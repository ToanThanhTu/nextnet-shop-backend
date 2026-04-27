using net_backend.Modules.Users.Contracts;
using net_backend.Modules.Users.Domain;

namespace net_backend.Modules.Users.Application.Commands;

/// <summary>
/// Orchestrates the three concerns of "logging in":
///  1. Validate credentials (delegated to the Authentication domain
///     service; throws ValidationException with errorCode
///     INVALID_CREDENTIALS on failure)
///  2. Issue a JWT (JwtTokenHelper)
///  3. Project to the LoginResponse DTO
///
/// Authentication is split out so other flows (e.g. confirm-password
/// before a destructive action) can verify credentials without
/// re-issuing a token.
/// </summary>
public class LoginHandler(Authentication authentication, JwtTokenHelper jwt)
{
    public async Task<LoginResponse> ExecuteAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await authentication.AuthenticateAsync(
            request.Email, request.Password, cancellationToken);

        var token = jwt.GenerateToken(user);
        return new LoginResponse(UserDto.FromEntity(user), token);
    }
}
