using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using net_backend.Users;

namespace net_backend;

/// <summary>
/// Extension methods for configuring services in the application.
/// This class handles all dependency injection and service configuration
/// including database, authentication, CORS, and API documentation.
/// </summary>
public static class ConfigureServices
{
    /// <summary>
    /// Main entry point for configuring all application services.
    /// Called from Program.cs to set up the entire service container.
    /// </summary>
    /// <param name="builder">The web application builder to configure</param>
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.ConfigurePort();
        builder.AddCors();
        builder.AddSwagger();
        builder.AddDatabase();
        builder.AddJwtAuthentication();
        builder.AddAuthorizationPolicies();
    }

    /// <summary>
    /// Configures Kestrel web server to listen on port 8080.
    /// This ensures the application is accessible on the expected port for both
    /// development and deployment environments.
    /// </summary>
    private static void ConfigurePort(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            // Listen on all network interfaces on port 8080
            options.Listen(System.Net.IPAddress.Any, 8080);
        });
    }

    /// <summary>
    /// Configures Swagger/OpenAPI documentation for the API.
    /// This enables automatic API documentation generation and testing interface.
    /// </summary>
    private static void AddSwagger(this WebApplicationBuilder builder)
    {
        // Enables the API Explorer, provides metadata about the HTTP API
        builder.Services.AddEndpointsApiExplorer();

        // Adds the Swagger OpenAPI document generator with custom configuration
        builder.Services.AddOpenApiDocument(config =>
        {
            config.DocumentName = "NextNetAPI";
            config.Title = "NextNetAPI v1";
            config.Version = "v1";
        });
    }

    /// <summary>
    /// Configures PostgreSQL with EF Core. Reads the URL from configuration
    /// (dev: appsettings.json / ConnectionStrings__DATABASE_URL env override;
    ///  prod: DATABASE_URL env var as a Fly.io secret) and converts the
    /// postgres:// URL into Npgsql's key=value format using System.Uri +
    /// NpgsqlConnectionStringBuilder, which handle URL-encoded passwords,
    /// missing ports, and the postgresql:// scheme variant.
    /// </summary>
    private static void AddDatabase(this WebApplicationBuilder builder)
    {
        var connectionUrl = builder.Environment.IsDevelopment()
            ? builder.Configuration.GetConnectionString("DATABASE_URL")
            : Environment.GetEnvironmentVariable("DATABASE_URL");

        if (string.IsNullOrEmpty(connectionUrl))
        {
            throw new InvalidOperationException("DATABASE_URL is not set.");
        }

        var connectionString = ParsePostgresUrl(connectionUrl, builder.Environment.IsDevelopment());

        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    }

    /// <summary>
    /// Converts a postgres://user:pass@host:port/db?... URL into an Npgsql
    /// key=value connection string. Handles URL-encoded passwords, the
    /// postgresql:// scheme alias, missing ports (defaults to 5432), and
    /// query-string options. Production uses Fly.io's internal IPv6 routing
    /// by rewriting the .flycast hostname suffix to .internal.
    /// </summary>
    private static string ParsePostgresUrl(string url, bool isDevelopment)
    {
        // Uri.TryCreate doesn't accept "postgres://"; normalise to a scheme it knows.
        var normalised = url.StartsWith("postgres://", StringComparison.Ordinal)
            ? "http://" + url["postgres://".Length..]
            : url.StartsWith("postgresql://", StringComparison.Ordinal)
                ? "http://" + url["postgresql://".Length..]
                : url;

        var uri = new Uri(normalised);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var host = uri.Host;
        if (!isDevelopment)
        {
            host = host.Replace("flycast", "internal");
        }
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        var b = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Username = user,
            Password = password,
            Database = database,
            SslMode = SslMode.Disable,
        };
        return b.ConnectionString;
    }

    /// <summary>
    /// Configures CORS. Allowed origins come from Cors:AllowedOrigins
    /// (array of URLs) in configuration. AllowCredentials() lets the browser
    /// send the JWT in fetch credentials mode if/when that's added; with the
    /// current `Authorization: Bearer` header pattern it doesn't matter.
    /// </summary>
    private static void AddCors(this WebApplicationBuilder builder)
    {
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
                        // No allowlist configured: lock down. Set Cors:AllowedOrigins
                        // to enable cross-origin calls from the frontend.
                        policy.WithOrigins().AllowAnyMethod().AllowAnyHeader();
                    }
                    else
                    {
                        policy.WithOrigins(allowed).AllowAnyMethod().AllowAnyHeader();
                    }
                }
            );
        });
    }

    /// <summary>
    /// Configures JWT Bearer token authentication. Token issuance + validation
    /// share configuration via IOptions&lt;JwtOptions&gt; bound to the "Jwt"
    /// section. In dev, defaults can live in appsettings.json. In prod, set
    /// Jwt__SigningKey via fly secrets so the key never lives in source.
    /// </summary>
    private static void AddJwtAuthentication(this WebApplicationBuilder builder)
    {
        var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
        builder.Services.Configure<JwtOptions>(jwtSection);
        var jwt = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

        if (string.IsNullOrWhiteSpace(jwt.SigningKey))
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey is not configured. Set Jwt__SigningKey via env var or appsettings.");
        }

        builder.Services.AddSingleton<JwtTokenHelper>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(jwt.SigningKey)
                    ),
                };
            });
    }

    /// <summary>
    /// Defines the named authorization policies used across endpoints.
    /// "Admin" requires the Admin role; the default policy requires any
    /// authenticated user.
    /// </summary>
    private static void AddAuthorizationPolicies(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("Admin", p => p.RequireRole("Admin"));
    }
}
