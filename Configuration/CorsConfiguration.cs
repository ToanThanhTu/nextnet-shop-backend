namespace net_backend.Configuration;

/// <summary>
/// CORS policy. Allowed origins come from Cors:AllowedOrigins (string array)
/// in configuration. Empty array = locked down. The named policy "AllowFrontend"
/// is later applied in ConfigureApp via app.UseCors("AllowFrontend").
/// </summary>
public static class CorsConfiguration
{
    public static WebApplicationBuilder AddCorsPolicy(this WebApplicationBuilder builder)
    {
        // Read Cors:AllowedOrigins from config. .Get<string[]>() binds the
        // section value to a typed string[]. The ?? Array.Empty<string>() is
        // a null-coalescing fallback for when the section is missing entirely.
        var allowed = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowFrontend",
                policy =>
                {
                    if (allowed.Length == 0)
                    {
                        // No allowlist: lock down. WithOrigins() with no args
                        // means "no origins allowed".
                        policy.WithOrigins().AllowAnyMethod().AllowAnyHeader();
                    }
                    else
                    {
                        policy.WithOrigins(allowed).AllowAnyMethod().AllowAnyHeader();
                    }
                }
            );
        });

        return builder;
    }
}
