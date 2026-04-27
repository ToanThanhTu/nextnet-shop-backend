using net_backend.Data.Types;

namespace net_backend.SubCategories.Domain;

/// <summary>
/// The SubCategory aggregate's persistence contract. SubCategories belong
/// to a parent Category (CategoryId FK); cross-aggregate validation lives
/// in the application handlers, not here.
/// </summary>
public interface ISubCategoryRepository
{
    Task<List<SubCategory>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<SubCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<byte[]?> GetImageAsync(int id, CancellationToken cancellationToken = default);
    Task<SubCategory> AddAsync(SubCategory subCategory, CancellationToken cancellationToken = default);
    Task UpdateAsync(SubCategory subCategory, CancellationToken cancellationToken = default);
    Task DeleteAsync(SubCategory subCategory, CancellationToken cancellationToken = default);
}
