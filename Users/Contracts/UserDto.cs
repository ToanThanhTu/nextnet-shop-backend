using net_backend.Data.Types;

namespace net_backend.Users.Contracts;

/// <summary>
/// Public-safe user shape. Excludes PasswordHash and the User's
/// CartItems/Orders aggregates — clients fetch those separately
/// through /cart and /orders.
/// </summary>
public record UserDto(
    int Id,
    string Name,
    string Email,
    string Role)
{
    public static UserDto FromEntity(User user) =>
        new(user.Id, user.Name, user.Email, user.Role);
}
