namespace net_backend.Users.Contracts;

public record LoginResponse(UserDto User, string Token);
