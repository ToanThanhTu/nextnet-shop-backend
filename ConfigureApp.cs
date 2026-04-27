using net_backend.Cart;
using net_backend.Orders;
using net_backend.Products;
using net_backend.Users;

namespace net_backend;

/// <summary>
/// Application request pipeline. Middleware order matters here:
/// each Use* call adds a middleware in the order requests flow through.
/// Endpoint maps (RegisterXxxEndpoints) only add routes to the table; the
/// runtime order is determined by the Use* placements above.
/// </summary>
public static class ConfigureApp
{
    public static Task Configure(this WebApplication app)
    {
        // 1. Catch anything thrown below us. Must be first so exceptions in
        //    auth, CORS, or the endpoints all hit the global handler.
        app.UseExceptionHandler();

        // 2. CORS before auth: preflight (OPTIONS) responses don't need a JWT.
        app.UseCors("AllowFrontend");

        // 3. Auth: parse the bearer token (UseAuthentication), then check
        //    policies declared at endpoint level (UseAuthorization).
        app.UseAuthentication();
        app.UseAuthorization();

        // 4. Swagger UI is dev-only.
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

        // 5. Endpoint maps.
        //    - MapControllers picks up classes annotated [ApiController] /
        //      [Route]; this is the destination shape.
        //    - RegisterXxxEndpoints are the legacy Minimal API maps still
        //      to migrate; both kinds coexist while we move features over.
        app.MapControllers();

        // Legacy Minimal API endpoints; remove each as its feature migrates
        // to a controller. Migrated: Categories, SubCategories.
        app.RegisterProductsEndpoints();
        app.RegisterUsersEndpoints();
        app.RegisterCartEndpoints();
        app.RegisterOrdersEndpoints();

        return Task.CompletedTask;
    }
}
