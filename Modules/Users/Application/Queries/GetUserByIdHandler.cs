using net_backend.Common.Exceptions;
using net_backend.Users.Contracts;
using net_backend.Users.Domain;

namespace net_backend.Users.Application.Queries;

public class GetUserByIdHandler(IUserRepository repo)
{
    public async Task<UserDto> ExecuteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await repo.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"User {id} not found.", "USER_NOT_FOUND");
        return UserDto.FromEntity(user);
    }
}
