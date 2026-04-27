using System.Security.Claims;
using net_backend.Common.Exceptions;

namespace net_backend.Common.Auth;

/// <summary>
/// Convenience extensions on ClaimsPrincipal for reading the authenticated
/// user's identity. Used by controllers / handlers that operate on "the
/// caller's own data" — never take the user id as a path parameter for that.
/// </summary>
public static class UserContextExtensions
{
    /// <summary>
    /// Extract the int user id from the NameIdentifier claim.
    /// Throws ForbiddenException if the principal is unauthenticated or the
    /// claim is missing or malformed; this should never happen behind
    /// [Authorize], but the throw makes the contract explicit.
    /// </summary>
    public static int GetRequiredUserId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new ForbiddenException("Authenticated user has no NameIdentifier claim.");

        return int.TryParse(raw, out var id)
            ? id
            : throw new ForbiddenException($"NameIdentifier claim '{raw}' is not a valid user id.");
    }
}
