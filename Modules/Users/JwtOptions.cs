namespace net_backend.Users;

/// <summary>
/// Strongly-typed configuration for JWT issuance and validation.
/// Bound from the "Jwt" config section.
/// In dev, defaults can live in appsettings.json; in prod, set via the
/// Jwt__SigningKey env var (Fly secret).
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
