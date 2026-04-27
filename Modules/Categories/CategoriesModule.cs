using net_backend.Categories.Application.Commands;
using net_backend.Categories.Application.Queries;
using net_backend.Categories.Domain;
using net_backend.Categories.Infrastructure;

namespace net_backend.Categories;

/// <summary>
/// DI registration for the Categories feature. Keeps the wiring close to
/// the feature it belongs to so adding/removing a feature is a one-line
/// change in the orchestrator (ConfigureServices).
/// </summary>
public static class CategoriesModule
{
    public static WebApplicationBuilder AddCategoriesFeature(this WebApplicationBuilder builder)
    {
        // Repository: scoped (one per request) because it depends on the scoped
        // DbContext. Don't use Singleton here.
        builder.Services.AddScoped<ICategoryRepository, EfCategoryRepository>();

        // Handlers: scoped so each request gets its own (and the repository
        // they hold transitively works correctly).
        builder.Services.AddScoped<ListCategoriesHandler>();
        builder.Services.AddScoped<GetCategoryByIdHandler>();
        builder.Services.AddScoped<CreateCategoryHandler>();
        builder.Services.AddScoped<UpdateCategoryHandler>();
        builder.Services.AddScoped<DeleteCategoryHandler>();

        return builder;
    }
}
