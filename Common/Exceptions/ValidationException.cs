namespace net_backend.Common.Exceptions;

/// <summary>
/// Throw when client input fails validation (missing fields, bad format,
/// out-of-range values). Maps to HTTP 400.
/// </summary>
public class ValidationException : AppException
{
    /// <summary>
    /// Optional per-field error map. Keys are field names; values are the
    /// validation messages. Surfaces in the ProblemDetails response under
    /// the "errors" extension.
    /// </summary>
    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    public ValidationException(string message, string errorCode = "VALIDATION_ERROR")
        : base(StatusCodes.Status400BadRequest, errorCode, message) { }

    public ValidationException(
        string message,
        IReadOnlyDictionary<string, string[]> errors,
        string errorCode = "VALIDATION_ERROR")
        : base(StatusCodes.Status400BadRequest, errorCode, message)
    {
        Errors = errors;
    }
}
