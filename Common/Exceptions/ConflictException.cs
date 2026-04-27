namespace net_backend.Common.Exceptions;

/// <summary>
/// Throw when the request conflicts with current resource state (duplicate
/// email on register, version mismatch, etc.). Maps to HTTP 409.
/// </summary>
public class ConflictException : AppException
{
    public ConflictException(string message, string errorCode = "CONFLICT")
        : base(StatusCodes.Status409Conflict, errorCode, message) { }
}
