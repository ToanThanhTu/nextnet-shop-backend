namespace net_backend;

/// <summary>
/// Application request pipeline. Middleware order matters here:
/// each Use* call adds a middleware in the order requests flow through.
/// All features are now MVC controllers picked up by MapControllers.
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

        // 5. Endpoint maps. MapControllers picks up classes annotated
        //    [ApiController] / [Route]; every feature now lives under
        //    its own *Controller and is routed automatically.
        app.MapControllers();

        return Task.CompletedTask;
    }
}
