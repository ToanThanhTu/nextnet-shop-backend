using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace net_backend.Common.Exceptions;

/// <summary>
/// Catches unhandled exceptions and produces an RFC 7807 ProblemDetails
/// response. Domain exceptions (subclasses of AppException) map to their
/// declared status code and surface a stable error code; anything else
/// becomes a generic 500 with no internal details leaked to the client.
///
/// Registered in DI via builder.Services.AddExceptionHandler&lt;...&gt;()
/// and activated by app.UseExceptionHandler() in the request pipeline.
/// </summary>
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            AppException app => BuildAppProblem(app),
            _ => BuildUnexpectedProblem(exception),
        };

        // Log unexpected (non-AppException) errors at Error level so they show
        // up in alerts; AppException is "expected" domain feedback, log Warning.
        if (exception is AppException)
        {
            logger.LogWarning(exception, "Domain exception: {Message}", exception.Message);
        }
        else
        {
            logger.LogError(exception, "Unhandled exception");
        }

        // Include a stack trace only in development, to aid debugging without
        // leaking internals to prod clients.
        if (environment.IsDevelopment())
        {
            problem.Extensions["stackTrace"] = exception.StackTrace;
        }

        problem.Instance = httpContext.Request.Path;
        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static ProblemDetails BuildAppProblem(AppException exception)
    {
        var problem = new ProblemDetails
        {
            Status = exception.StatusCode,
            Title = TitleFor(exception.StatusCode),
            Detail = exception.Message,
        };
        problem.Extensions["errorCode"] = exception.ErrorCode;

        // Validation errors carry per-field messages; surface them under "errors".
        if (exception is ValidationException { Errors: { } errors })
        {
            problem.Extensions["errors"] = errors;
        }
        return problem;
    }

    private static ProblemDetails BuildUnexpectedProblem(Exception _) => new()
    {
        Status = StatusCodes.Status500InternalServerError,
        Title = "Internal Server Error",
        Detail = "An unexpected error occurred.",
    };

    private static string TitleFor(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        _ => "Error",
    };
}
