using net_backend.Common.Exceptions;

namespace net_backend.Configuration;

/// <summary>
/// Registers the global exception handler. Pair with app.UseExceptionHandler()
/// in the request pipeline (ConfigureApp).
/// </summary>
public static class ExceptionHandlingConfiguration
{
    public static WebApplicationBuilder AddGlobalExceptionHandler(this WebApplicationBuilder builder)
    {
        // Register the handler in DI. UseExceptionHandler() picks up registered
        // IExceptionHandler instances automatically.
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // AddProblemDetails plugs in the default ProblemDetails writer. Without
        // it, our handler still works (we write JSON ourselves), but framework
        // pieces that emit ProblemDetails (e.g. Status400 from validation) get
        // a consistent shape.
        builder.Services.AddProblemDetails();

        return builder;
    }
}
