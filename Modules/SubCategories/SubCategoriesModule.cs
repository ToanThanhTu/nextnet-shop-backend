using net_backend.Modules.SubCategories.Application.Commands;
using net_backend.Modules.SubCategories.Application.Queries;
using net_backend.Modules.SubCategories.Domain;
using net_backend.Modules.SubCategories.Infrastructure;

namespace net_backend.Modules.SubCategories;

public static class SubCategoriesModule
{
    public static WebApplicationBuilder AddSubCategoriesFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ISubCategoryRepository, EfSubCategoryRepository>();
        builder.Services.AddScoped<ListSubCategoriesHandler>();
        builder.Services.AddScoped<GetSubCategoryByIdHandler>();
        builder.Services.AddScoped<CreateSubCategoryHandler>();
        builder.Services.AddScoped<UpdateSubCategoryHandler>();
        builder.Services.AddScoped<DeleteSubCategoryHandler>();
        return builder;
    }
}
