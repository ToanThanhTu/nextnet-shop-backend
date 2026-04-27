using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using net_backend.Users;

namespace net_backend.Configuration;

/// <summary>
/// JWT bearer authentication + named authorization policies.
/// Issuance and validation share the same JwtOptions instance via DI,
/// so they cannot drift apart. JwtTokenHelper is registered as a
/// singleton; inject it as a parameter into login-style handlers.
/// </summary>
public static class AuthConfiguration
{
    public static WebApplicationBuilder AddJwtAuthentication(this WebApplicationBuilder builder)
    {
        // Bind the "Jwt" config section to JwtOptions and register it for DI.
        // Anyone taking IOptions<JwtOptions> gets the populated instance.
        var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
        builder.Services.Configure<JwtOptions>(jwtSection);

        // Read the section eagerly to validate it at startup. We want the app
        // to fail fast if the signing key is missing rather than crash on the
        // first login attempt.
        var jwt = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwt.SigningKey))
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey is not configured. Set Jwt__SigningKey via env var or appsettings.");
        }

        // Our own helper for issuing tokens. Singleton: stateless, thread-safe.
        builder.Services.AddSingleton<JwtTokenHelper>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(jwt.SigningKey)
                    ),
                };
            });

        return builder;
    }

    /// <summary>
    /// Defines the named authorization policies used at endpoint registration
    /// (.RequireAuthorization("Admin")). Add new policies here.
    /// </summary>
    public static WebApplicationBuilder AddAuthorizationPolicies(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("Admin", p => p.RequireRole("Admin"));

        return builder;
    }
}
