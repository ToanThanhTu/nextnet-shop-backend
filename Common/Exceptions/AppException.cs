namespace net_backend.Common.Exceptions;

/// <summary>
/// Base class for domain exceptions that should map to a specific HTTP status
/// and carry a stable error code for clients to switch on. Inherit from this
/// (NotFoundException, ConflictException, etc.) instead of throwing the
/// generic Exception type, so the global handler can produce a consistent
/// ProblemDetails response.
/// </summary>
public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }

    protected AppException(int statusCode, string errorCode, string message, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
