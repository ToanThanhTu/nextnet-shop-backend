namespace net_backend.Configuration;

/// <summary>
/// OpenAPI / Swagger document generation. NSwag scans the running app's
/// endpoints and produces an OpenAPI 3 document at /swagger/{name}/swagger.json.
/// The Swagger UI middleware (registered in ConfigureApp) serves it as
/// interactive docs at /swagger.
/// </summary>
public static class OpenApiConfiguration
{
    public static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
    {
        // AddEndpointsApiExplorer() makes Minimal API endpoints visible to the
        // API explorer. NSwag/Swashbuckle/etc. read from this to find routes.
        builder.Services.AddEndpointsApiExplorer();

        // NSwag's OpenAPI document generator. The lambda configures metadata
        // that ends up in the generated openapi.json (info.title, version, etc.).
        builder.Services.AddOpenApiDocument(config =>
        {
            config.DocumentName = "NextNetAPI";
            config.Title = "NextNetAPI v1";
            config.Version = "v1";
        });

        return builder;
    }
}
