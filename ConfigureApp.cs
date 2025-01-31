using net_backend.Cart;
using net_backend.Categories;
using net_backend.Orders;
using net_backend.Products;
using net_backend.Users;

namespace net_backend;

public static class ConfigureApp
{
    public static Task Configure(this WebApplication app)
    {
        app.UseCors("AllowFrontend");

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

        // Register Users endpoints
        app.RegisterUsersEndpoints();

        // Register Cart endpoints
        app.RegisterCartEndpoints();

        // Register Orders endpoints
        app.RegisterOrdersEndpoints();

        app.UseAuthentication();
        //app.UseAuthorization();

        return Task.CompletedTask;
    }
}
