using net_backend.Common.Exceptions;
using net_backend.Data.Types;

namespace net_backend.Users.Domain;

/// <summary>
/// Domain service for "verify a user's email + password." Returns the
/// User entity on success; throws a generic UnauthorizedException (401)
/// on failure so the controller surface looks identical regardless of
/// whether the email was unknown or the password was wrong (no user
/// enumeration).
///
/// Uses constant-time bcrypt verification: when the user isn't found,
/// we still run a dummy verify so the request timing doesn't reveal
/// whether the email exists.
/// </summary>
public class Authentication(IUserRepository repo)
{
    // A precomputed bcrypt hash so the dummy verify path runs the same
    // amount of work as a real verify. Don't try to match a real
    // password to it; it's cryptographic noise.
    private const string DummyHash = "$2a$10$abcdefghijklmnopqrstuv";

    public async Task<User> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await repo.FindByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            // Run a dummy verify so timing doesn't leak whether the
            // email exists in the database.
            BCrypt.Net.BCrypt.Verify(password, DummyHash);
            throw new UnauthorizedException("Invalid email or password.", "INVALID_CREDENTIALS");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.", "INVALID_CREDENTIALS");
        }

        return user;
    }
}
