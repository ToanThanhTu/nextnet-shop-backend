using net_backend.Configuration;

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
            .AddAuthorizationPolicies();
    }
}
