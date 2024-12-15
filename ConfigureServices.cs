using Microsoft.EntityFrameworkCore;

public static class ConfigureServices
{
  public static void AddServices(this WebApplicationBuilder builder)
  {
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
      opt.UseInMemoryDatabase("CategoryList");
      opt.UseInMemoryDatabase("SubCategoryList");
      opt.UseInMemoryDatabase("ProductList");
    });

    // enables displaying database-related exceptions
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
  }
}