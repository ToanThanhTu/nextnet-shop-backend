namespace net_backend.Common.Exceptions;

/// <summary>
/// Throw when authentication fails (invalid credentials, expired token,
/// missing token where one was required and middleware didn't catch it).
/// Maps to HTTP 401.
///
/// Don't use this for "authenticated but not authorized" — that's
/// ForbiddenException (403).
/// </summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message, string errorCode = "UNAUTHORIZED")
        : base(StatusCodes.Status401Unauthorized, errorCode, message) { }
}
