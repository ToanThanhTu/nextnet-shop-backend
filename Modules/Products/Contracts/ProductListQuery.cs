namespace net_backend.Modules.Products.Contracts;

/// <summary>
/// Query string parameters for the list endpoints. Bound from the URL via
/// [FromQuery] on the controller action. All fields optional; sensible
/// defaults applied in the handlers.
/// </summary>
public record ProductListQuery(
    string? Category = null,
    string? Subcategory = null,
    decimal? PriceMin = null,
    decimal? PriceMax = null,
    string? SortBy = null,
    int Limit = 12,
    int Page = 1)
{
    /// <summary>Clamp limit to [1, 100] and page to >= 1, returning a
    /// safe normalized copy.</summary>
    public ProductListQuery Normalised() => this with
    {
        Limit = Math.Clamp(Limit, 1, 100),
        Page = Math.Max(Page, 1),
    };
}
