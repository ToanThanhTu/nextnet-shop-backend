using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using net_backend.Modules.Users.Application.Commands;
using net_backend.Modules.Users.Application.Queries;
using net_backend.Modules.Users.Contracts;

namespace net_backend.Modules.Users;

[ApiController]
[Route("users")]
public class UsersController(
    ListUsersHandler listHandler,
    GetUserByIdHandler getByIdHandler,
    RegisterUserHandler registerHandler,
    CreateAdminHandler createAdminHandler,
    LoginHandler loginHandler) : ControllerBase
{
    [HttpGet("")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<List<UserDto>>> List(CancellationToken cancellationToken)
        => Ok(await listHandler.ExecuteAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetById(int id, CancellationToken cancellationToken)
        => Ok(await getByIdHandler.ExecuteAsync(id, cancellationToken));

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await registerHandler.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPost("admin")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<UserDto>> CreateAdmin(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await createAdminHandler.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
        => Ok(await loginHandler.ExecuteAsync(request, cancellationToken));
}
