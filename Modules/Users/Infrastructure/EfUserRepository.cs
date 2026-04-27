using Microsoft.EntityFrameworkCore;
using net_backend.Data.Types;
using net_backend.Modules.Users.Domain;

namespace net_backend.Modules.Users.Infrastructure;

public class EfUserRepository(AppDbContext db) : IUserRepository
{
    public Task<List<User>> ListAllAsync(CancellationToken cancellationToken = default)
        => db.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .ToListAsync(cancellationToken);

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        => db.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => db.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return user;
    }
}
