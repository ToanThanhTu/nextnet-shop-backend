public static class ConfigureApp
{
  public static async Task Configure(this WebApplication app)
  {
    // Enable Swagger middleware for testing in development
    if (app.Environment.IsDevelopment())
    {
      app.UseOpenApi();
      app.UseSwaggerUi(config =>
      {
        config.DocumentTitle = "NextNetAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
      });
    }

    // Register Categories endpoints
    app.RegisterCategoriesEndpoints();

    // Register SubCategories endpoints
    app.RegisterSubCategoriesEndpoints();

    // Register Products endpoints
    app.RegisterProductsEndpoints();
  }
}
