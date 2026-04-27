using net_backend.Modules.Users.Application.Commands;
using net_backend.Modules.Users.Application.Queries;
using net_backend.Modules.Users.Domain;
using net_backend.Modules.Users.Infrastructure;

namespace net_backend.Modules.Users;

public static class UsersModule
{
    public static WebApplicationBuilder AddUsersFeature(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserRepository, EfUserRepository>();
        builder.Services.AddScoped<Authentication>();

        builder.Services.AddScoped<ListUsersHandler>();
        builder.Services.AddScoped<GetUserByIdHandler>();
        builder.Services.AddScoped<RegisterUserHandler>();
        builder.Services.AddScoped<CreateAdminHandler>();
        builder.Services.AddScoped<LoginHandler>();

        return builder;
    }
}
