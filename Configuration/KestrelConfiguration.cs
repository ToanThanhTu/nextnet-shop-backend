namespace net_backend.Configuration;

/// <summary>
/// Kestrel web server configuration. Binds to port 8080 on all
/// interfaces so the container/host port mapping works without
/// extra wiring.
/// </summary>
public static class KestrelConfiguration
{
    public static WebApplicationBuilder ConfigurePort(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(System.Net.IPAddress.Any, 8080);
        });
        return builder;
    }
}
