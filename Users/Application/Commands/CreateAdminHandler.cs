using net_backend.Common.Exceptions;
using net_backend.Data.Types;
using net_backend.Users.Contracts;
using net_backend.Users.Domain;

namespace net_backend.Users.Application.Commands;

/// <summary>
/// Create an admin user. Same shape as register, different role assignment;
/// admin-only at the controller layer.
/// </summary>
public class CreateAdminHandler(IUserRepository repo)
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
            Role = "Admin",
        };

        var saved = await repo.AddAsync(user, cancellationToken);
        return UserDto.FromEntity(saved);
    }
}
