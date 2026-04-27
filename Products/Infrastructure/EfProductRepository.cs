using Microsoft.EntityFrameworkCore;
using net_backend.Data.Types;
using net_backend.Products.Contracts;
using net_backend.Products.Domain;

namespace net_backend.Products.Infrastructure;

public class EfProductRepository(AppDbContext db) : IProductRepository
{
    public async Task<(List<ProductDto> items, int totalItems)> ListAsync(
        ProductListQuery query, CancellationToken cancellationToken = default)
    {
        var q = query.Normalised();

        // Join through SubCategory + Category so we can filter by either slug.
        // EF translates the joined where clauses to a single SQL query.
        var filtered = db.Products
            .AsNoTracking()
            .Join(db.SubCategories,
                p => p.SubCategoryId, sc => sc.Id,
                (p, sc) => new { Product = p, SubCategory = sc })
            .Join(db.Categories,
                ps => ps.SubCategory.CategoryId, c => c.Id,
                (ps, c) => new { ps.Product, ps.SubCategory, Category = c })
            .Where(x =>
                (q.Category == null || x.Category.Slug == q.Category) &&
                (q.Subcategory == null || x.SubCategory.Slug == q.Subcategory) &&
                (q.PriceMin == null || x.Product.SalePrice >= q.PriceMin) &&
                (q.PriceMax == null || x.Product.SalePrice <= q.PriceMax))
            .Select(x => x.Product);

        return await PaginateAsync(ApplySort(filtered, q.SortBy), q, cancellationToken);
    }

    public async Task<(List<ProductDto> items, int totalItems)> ListOnSaleAsync(
        ProductListQuery query, CancellationToken cancellationToken = default)
    {
        var q = query.Normalised();
        var onSale = db.Products.AsNoTracking().Where(p => p.Sale > 0);
        return await PaginateAsync(ApplySort(onSale, q.SortBy), q, cancellationToken);
    }

    public async Task<(List<ProductDto> items, int totalItems)> ListBestsellersAsync(
        ProductListQuery query, CancellationToken cancellationToken = default)
    {
        var q = query.Normalised();
        var ordered = db.Products
            .AsNoTracking()
            .Where(p => p.Sold != null && p.Sold > 0)
            .OrderByDescending(p => p.Sold);
        return await PaginateAsync(ordered, q, cancellationToken);
    }

    public Task<List<ProductDto>> ListTopDealsAsync(int limit, CancellationToken cancellationToken = default)
        => db.Products
            .AsNoTracking()
            .Where(p => p.Sale > 0)
            .OrderByDescending(p => p.Sale)
            .Take(limit)
            .Select(ProductDto.Projection)
            .ToListAsync(cancellationToken);

    public Task<List<ProductDto>> SearchByTitleAsync(string query, CancellationToken cancellationToken = default)
        => db.Products
            .AsNoTracking()
            .Where(p => EF.Functions.ILike(p.Title, $"%{query}%"))
            .Select(ProductDto.Projection)
            .ToListAsync(cancellationToken);

    public Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(ProductDto.Projection)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Product?> GetBySlugWithHierarchyAsync(string slug, CancellationToken cancellationToken = default)
        => db.Products
            .AsNoTracking()
            .Include(p => p.SubCategory!)
                .ThenInclude(sc => sc.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);

    public async Task<List<ProductDto>> ListSimilarAsync(int productId, CancellationToken cancellationToken = default)
    {
        // First, find which subcategory the input product belongs to.
        var subCategoryId = await db.Products
            .Where(p => p.Id == productId)
            .Select(p => (int?)p.SubCategoryId)
            .FirstOrDefaultAsync(cancellationToken);

        if (subCategoryId is null) return [];

        return await db.Products
            .AsNoTracking()
            .Where(p => p.SubCategoryId == subCategoryId && p.Id != productId)
            .Select(ProductDto.Projection)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ProductDto>> ListByUserPurchaseHistoryAsync(
        int userId, CancellationToken cancellationToken = default)
    {
        // Distinct subcategories the user has previously bought from.
        var subCategoryIds = await db.Orders
            .Where(o => o.UserId == userId)
            .SelectMany(o => o.OrderItems!)
            .Select(oi => oi.Product!.SubCategoryId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (subCategoryIds.Count == 0) return [];

        return await db.Products
            .AsNoTracking()
            .Where(p => subCategoryIds.Contains(p.SubCategoryId))
            .Select(ProductDto.Projection)
            .ToListAsync(cancellationToken);
    }

    public Task<byte[]?> GetImageAsync(int id, CancellationToken cancellationToken = default)
        => db.Products
            .Where(p => p.Id == id)
            .Select(p => p.Image)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Product?> GetEntityByIdAsync(int id, CancellationToken cancellationToken = default)
        => db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        return product;
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        db.Products.Remove(product);
        await db.SaveChangesAsync(cancellationToken);
    }

    // ----- private helpers -----

    private static IQueryable<Product> ApplySort(IQueryable<Product> q, string? sortBy) => sortBy switch
    {
        "priceLowHigh" => q.OrderBy(p => p.SalePrice),
        "priceHighLow" => q.OrderByDescending(p => p.SalePrice),
        _ => q.OrderBy(p => p.Id),
    };

    private static async Task<(List<ProductDto> items, int totalItems)> PaginateAsync(
        IQueryable<Product> source, ProductListQuery q, CancellationToken cancellationToken)
    {
        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((q.Page - 1) * q.Limit)
            .Take(q.Limit)
            .Select(ProductDto.Projection)
            .ToListAsync(cancellationToken);
        return (items, total);
    }
}
