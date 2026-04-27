using Microsoft.EntityFrameworkCore;
using net_backend.Modules.Categories.Domain;
using net_backend.Data.Types;

namespace net_backend.Modules.Categories.Infrastructure;

/// <summary>
/// EF Core implementation of ICategoryRepository. The only place in the
/// Categories feature that knows about AppDbContext or LINQ-to-EF.
/// Registered as Scoped (one per request) in DI alongside the DbContext.
/// </summary>
public class EfCategoryRepository(AppDbContext db) : ICategoryRepository
{
    public Task<List<Category>> ListWithSubCategoriesAsync(CancellationToken cancellationToken = default)
        => db.Categories
            .AsNoTracking()                       // read-only path; skip change tracking
            .Include(c => c.SubCategories)        // eager-load children
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);

    public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<byte[]?> GetImageAsync(int id, CancellationToken cancellationToken = default)
    {
        // Project to the Image bytes only so EF doesn't fetch the whole row.
        // FirstOrDefaultAsync on a struct/byte[]? is a bit awkward; we use a
        // Where + Select + FirstOrDefault explicitly.
        return await db.Categories
            .Where(c => c.Id == id)
            .Select(c => c.Image)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return category; // EF populated category.Id after SaveChanges
    }

    public Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(Category category, CancellationToken cancellationToken = default)
    {
        db.Categories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
    }
}
