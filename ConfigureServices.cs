using System.Net;
using Microsoft.EntityFrameworkCore;

namespace net_backend;

public static class ConfigureServices
{
  public static void AddServices(this WebApplicationBuilder builder)
  {
    builder.AddCors();
    builder.AddSwagger();
    builder.AddDatabase();
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
    // adds the database context to the dependency injection (DI) container
    builder.Services.AddDbContext<AppDbContext>(opt =>
    {
      opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
      );
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
}
