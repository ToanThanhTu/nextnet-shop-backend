namespace net_backend.Modules.Users.Contracts;

public record LoginResponse(UserDto User, string Token);
