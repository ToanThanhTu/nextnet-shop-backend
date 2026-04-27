namespace net_backend.Common.Exceptions;

/// <summary>
/// Throw when the user is authenticated but not allowed to perform the
/// action (e.g. trying to update someone else's order). Maps to HTTP 403.
///
/// Don't use this for missing authentication; let the JWT middleware
/// handle that case (returns 401 automatically).
/// </summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message, string errorCode = "FORBIDDEN")
        : base(StatusCodes.Status403Forbidden, errorCode, message) { }
}
