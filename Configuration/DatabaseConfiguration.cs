using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace net_backend.Configuration;

/// <summary>
/// PostgreSQL + EF Core registration. Reads the connection URL from
/// configuration (dev: appsettings.json / ConnectionStrings__DATABASE_URL
/// env override; prod: bare DATABASE_URL env var as a Fly secret) and
/// converts the postgres:// URL into Npgsql's key=value format.
/// </summary>
public static class DatabaseConfiguration
{
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        var connectionUrl = builder.Environment.IsDevelopment()
            ? builder.Configuration.GetConnectionString("DATABASE_URL")
            : Environment.GetEnvironmentVariable("DATABASE_URL");

        if (string.IsNullOrEmpty(connectionUrl))
        {
            throw new InvalidOperationException("DATABASE_URL is not set.");
        }

        var connectionString = ParsePostgresUrl(connectionUrl, builder.Environment.IsDevelopment());

        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        return builder;
    }

    /// <summary>
    /// Converts a postgres://user:pass@host:port/db?... URL into an Npgsql
    /// key=value connection string. Handles URL-encoded passwords, the
    /// postgresql:// scheme alias, missing ports (defaults to 5432), and
    /// query-string options. Production rewrites the .flycast hostname suffix
    /// to .internal so Fly's IPv6 6PN routing kicks in.
    /// </summary>
    private static string ParsePostgresUrl(string url, bool isDevelopment)
    {
        // Uri.TryCreate doesn't accept "postgres://"; normalise to a scheme it knows.
        var normalised = url.StartsWith("postgres://", StringComparison.Ordinal)
            ? "http://" + url["postgres://".Length..]
            : url.StartsWith("postgresql://", StringComparison.Ordinal)
                ? "http://" + url["postgresql://".Length..]
                : url;

        var uri = new Uri(normalised);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var host = uri.Host;
        if (!isDevelopment)
        {
            host = host.Replace("flycast", "internal");
        }
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        var b = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Username = user,
            Password = password,
            Database = database,
            SslMode = SslMode.Disable,
        };
        return b.ConnectionString;
    }
}
