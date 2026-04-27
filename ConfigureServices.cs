using net_backend.Categories;
using net_backend.Configuration;
using net_backend.SubCategories;

namespace net_backend;

/// <summary>
/// Service-container orchestration. Each concern lives in its own
/// extension method file under net_backend.Configuration; this class
/// just composes them in the order Program.cs expects.
/// </summary>
public static class ConfigureServices
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder
            .ConfigurePort()
            .AddCorsPolicy()
            .AddSwagger()
            .AddDatabase()
            .AddJwtAuthentication()
            .AddAuthorizationPolicies()
            .AddGlobalExceptionHandler();

        // MVC controllers. Coexists with Minimal API endpoints during migration.
        // [ApiController] enables automatic 400 ProblemDetails on model-binding
        // failures, model validation, and convention-based [FromBody] inference.
        builder.Services.AddControllers();

        // Per-feature DI registration. Each feature owns its own AddXxxFeature
        // extension; this keeps controllers, handlers, and repositories close
        // to the code that uses them.
        builder.AddCategoriesFeature();
        builder.AddSubCategoriesFeature();
    }
}
