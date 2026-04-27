using Microsoft.EntityFrameworkCore;
using net_backend.Data.Types;
using net_backend.SubCategories.Domain;

namespace net_backend.SubCategories.Infrastructure;

public class EfSubCategoryRepository(AppDbContext db) : ISubCategoryRepository
{
    public Task<List<SubCategory>> ListAllAsync(CancellationToken cancellationToken = default)
        => db.SubCategories
            .AsNoTracking()
            .OrderBy(sc => sc.Id)
            .ToListAsync(cancellationToken);

    public Task<SubCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => db.SubCategories.FirstOrDefaultAsync(sc => sc.Id == id, cancellationToken);

    public async Task<byte[]?> GetImageAsync(int id, CancellationToken cancellationToken = default)
        => await db.SubCategories
            .Where(sc => sc.Id == id)
            .Select(sc => sc.Image)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<SubCategory> AddAsync(SubCategory subCategory, CancellationToken cancellationToken = default)
    {
        db.SubCategories.Add(subCategory);
        await db.SaveChangesAsync(cancellationToken);
        return subCategory;
    }

    public Task UpdateAsync(SubCategory subCategory, CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(SubCategory subCategory, CancellationToken cancellationToken = default)
    {
        db.SubCategories.Remove(subCategory);
        await db.SaveChangesAsync(cancellationToken);
    }
}
