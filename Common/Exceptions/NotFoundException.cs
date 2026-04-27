namespace net_backend.Common.Exceptions;

/// <summary>
/// Throw when a resource the client requested doesn't exist. Maps to HTTP 404.
/// </summary>
public class NotFoundException : AppException
{
    public NotFoundException(string message, string errorCode = "NOT_FOUND")
        : base(StatusCodes.Status404NotFound, errorCode, message) { }
}
