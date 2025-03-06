using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace net_backend;

public static class ConfigureServices
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.AddCors();
        builder.AddSwagger();
        builder.AddDatabase();
        builder.AddJwtAuthentication();
    }

    private static void AddSwagger(this WebApplicationBuilder builder)
    {
        // Enables the API Explorer, provides metadata about the HTTP API
        builder.Services.AddEndpointsApiExplorer();

        // Adds the Swagger OpenAPI document generator
        builder.Services.AddOpenApiDocument(config =>
        {
            config.DocumentName = "NextNetAPI";
            config.Title = "NextNetAPI v1";
            config.Version = "v1";
        });
    }

    private static void AddDatabase(this WebApplicationBuilder builder)
    {
        var connection = String.Empty;

        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddEnvironmentVariables().AddJsonFile("appsettings.Development.json");
            connection = builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");
        }
        else
        {
            connection = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTIONSTRING");
        }

        // adds the database context to the dependency injection (DI) container
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connection);
        });

        // enables displaying database-related exceptions
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    }

    private static void AddCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowFrontend",
                policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            );
        });
    }

    private static void AddJwtAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "Trevor",
                    ValidAudience = "Trevor",
                    IssuerSigningKey = new SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes("my_super_secret_key_1234567890abcdef")
                    )
                };
            });
    }
}
