using net_backend.Modules.Products.Contracts;
using net_backend.Modules.Products.Domain;

namespace net_backend.Modules.Products.Application.Queries;

/// <summary>
/// Recommendations based on the user's order history. Falls back to top
/// deals when the user has no orders, so the response is never empty for
/// a fresh account (matches the legacy behaviour).
/// </summary>
public class GetPersonalRecommendationsHandler(IProductRepository repo)
{
    public async Task<List<ProductDto>> ExecuteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var byHistory = await repo.ListByUserPurchaseHistoryAsync(userId, cancellationToken);
        if (byHistory.Count > 0) return byHistory;

        return await repo.ListTopDealsAsync(limit: 4, cancellationToken);
    }
}
