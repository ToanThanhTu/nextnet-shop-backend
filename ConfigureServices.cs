using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL;

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
        // Configure server to listen on port 8080
        builder.ConfigurePort();
        
        // Enable Cross-Origin Resource Sharing for frontend communication
        builder.AddCors();
        
        // Set up Swagger/OpenAPI documentation
        builder.AddSwagger();
        
        // Configure PostgreSQL database with Entity Framework
        builder.AddDatabase();
        
        // Set up JWT Bearer token authentication
        builder.AddJwtAuthentication();
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
    /// Configures PostgreSQL database connection with Entity Framework Core.
    /// Handles both development and production environments with different connection sources.
    /// Includes complex URL parsing logic for Fly.io PostgreSQL URL format.
    /// </summary>
    private static void AddDatabase(this WebApplicationBuilder builder)
    {
        var connection = String.Empty;
        var connectionUrl = String.Empty;

        // Environment-specific connection string retrieval.
        // Dev: ConnectionStrings:DATABASE_URL — via appsettings.json by default,
        //      override-able by the env var ConnectionStrings__DATABASE_URL (e.g. inside Docker).
        // Prod: bare DATABASE_URL env var (Fly.io secret).
        if (builder.Environment.IsDevelopment())
        {
            connectionUrl = builder.Configuration.GetConnectionString("DATABASE_URL");
        }
        else
        {
            connectionUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        }

        // Parse PostgreSQL URL format (postgres://user:pass@host:port/database) to Npgsql connection string
        if (!string.IsNullOrEmpty(connectionUrl))
        {
            // Remove the protocol prefix (postgres://) to get: user:pass@host:port/database
            connectionUrl = connectionUrl.Replace("postgres://", string.Empty);

            // Split on '@' to separate credentials from host information
            // Format: user:pass @ host:port/database
            var pgUserPass = connectionUrl.Split('@')[0];     // user:pass
            var pgHostPortDb = connectionUrl.Split('@')[1];   // host:port/database

            // Extract user credentials
            var pgUser = pgUserPass.Split(':')[0];    // username
            var pgPass = pgUserPass.Split(':')[1];    // password

            // Split host information to separate host:port from database
            var pgHostPort = pgHostPortDb.Split('/')[0];              // host:port
            var pgDb = pgHostPortDb.Split('/')[1].Split('?')[0];      // database (remove query params if any)

            // Extract host and port
            var pgHost = pgHostPort.Split(':')[0];    // hostname
            var pgPort = pgHostPort.Split(':')[1];    // port number

            // Fly.io specific: Replace flycast with internal for proper hostname resolution
            // This is required for Fly.io's internal networking
            var updatedHost = pgHost.Replace("flycast", "internal");

            // Construct the Npgsql connection string format
            if (builder.Environment.IsDevelopment())
            {
                // Development: Use original host for local or external database
                connection = $"Host={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SslMode=Disable;Trust Server Certificate=true;";
            }
            else
            {
                // Production: Use Fly.io internal networking hostname
                connection = $"Host={updatedHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SslMode=Disable;Trust Server Certificate=true;";
            }

            // Log the connection string for debugging (password will be visible in logs)
            Debug.WriteLine($"connection string: {connection}");
        }
        else
        {
            // No connection string found - this is a critical configuration error
            throw new InvalidOperationException("DATABASE_URL environment variable is not set.");
        }

        // Register the database context in the dependency injection container
        // This makes AppDbContext available for injection throughout the application
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            // Configure Entity Framework to use PostgreSQL with the parsed connection string
            options.UseNpgsql(connection);
        });

        // Enable detailed database exception pages in development for better debugging
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    }

    /// <summary>
    /// Configures Cross-Origin Resource Sharing (CORS) to allow frontend communication.
    /// Currently configured to allow all origins, methods, and headers for development.
    /// TODO: Restrict to specific frontend domains in production for security.
    /// </summary>
    private static void AddCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowFrontend",
                policy =>
                {
                    // Allow any origin, method, and header
                    // WARNING: This is permissive and should be restricted in production
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            );
        });
    }

    /// <summary>
    /// Configures JWT Bearer token authentication for API endpoints.
    /// Sets up token validation parameters including issuer, audience, and signing key.
    /// TODO: Move hardcoded values to configuration for better security.
    /// </summary>
    private static void AddJwtAuthentication(this WebApplicationBuilder builder)
    {
        // Configure JWT Bearer authentication as the default authentication scheme
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Configure how incoming JWT tokens should be validated
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Validate that the token was issued by the expected issuer
                    ValidateIssuer = true,
                    // Validate that the token is intended for the expected audience
                    ValidateAudience = true,
                    // Validate that the token hasn't expired
                    ValidateLifetime = true,
                    // Validate the signature to ensure token hasn't been tampered with
                    ValidateIssuerSigningKey = true,
                    
                    // TODO: Move these to configuration for environment-specific values
                    ValidIssuer = "Trevor",
                    ValidAudience = "Trevor",
                    
                    // Symmetric key used to sign and validate JWT tokens
                    // WARNING: This should be moved to environment variables or secure configuration
                    IssuerSigningKey = new SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes("my_super_secret_key_1234567890abcdef")
                    )
                };
            });
    }
}
