using net_backend.Data.Types;

namespace net_backend.Modules.Categories.Domain;

/// <summary>
/// The Category aggregate's persistence contract. Application-layer
/// handlers depend on this interface, never on a concrete EF/SQL/HTTP
/// implementation. Swap implementations (e.g. for tests) by registering
/// a different class against this type in DI.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>List all categories with their SubCategories eagerly loaded.</summary>
    Task<List<Category>> ListWithSubCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>Find a single category by id; returns null if not found.</summary>
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Return only the image bytes for the given id; null if missing.</summary>
    Task<byte[]?> GetImageAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Insert a new category and return it with its assigned id.</summary>
    Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>Persist changes to a tracked category.</summary>
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>Remove a tracked category.</summary>
    Task DeleteAsync(Category category, CancellationToken cancellationToken = default);
}
