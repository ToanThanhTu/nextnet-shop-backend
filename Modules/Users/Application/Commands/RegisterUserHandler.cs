using net_backend.Common.Exceptions;
using net_backend.Data.Types;
using net_backend.Users.Contracts;
using net_backend.Users.Domain;

namespace net_backend.Users.Application.Commands;

/// <summary>
/// Register a regular (non-admin) user. Hashes the password with bcrypt
/// (default work factor 10) and assigns the "User" role.
/// </summary>
public class RegisterUserHandler(IUserRepository repo)
{
    public async Task<UserDto> ExecuteAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await repo.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new ConflictException(
                "Email is already registered.",
                "EMAIL_TAKEN");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User",
        };

        var saved = await repo.AddAsync(user, cancellationToken);
        return UserDto.FromEntity(saved);
    }
}
