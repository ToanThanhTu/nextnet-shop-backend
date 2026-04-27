using net_backend.Data.Types;

namespace net_backend.Users.Domain;

public interface IUserRepository
{
    /// <summary>List every user. Admin-only at the controller layer.</summary>
    Task<List<User>> ListAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Find by id; returns null if not found.</summary>
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Find by email (case-sensitive); returns null if no match.</summary>
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Returns true if a user with this email already exists.</summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Insert a new user; returns it with assigned id.</summary>
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
}
