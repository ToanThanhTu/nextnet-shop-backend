using net_backend.Users.Contracts;
using net_backend.Users.Domain;

namespace net_backend.Users.Application.Queries;

public class ListUsersHandler(IUserRepository repo)
{
    public async Task<List<UserDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var users = await repo.ListAllAsync(cancellationToken);
        return users.Select(UserDto.FromEntity).ToList();
    }
}
