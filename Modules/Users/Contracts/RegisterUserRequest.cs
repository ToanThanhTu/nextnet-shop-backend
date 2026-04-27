using System.ComponentModel.DataAnnotations;

namespace net_backend.Modules.Users.Contracts;

public record RegisterUserRequest(
    [Required, StringLength(50, MinimumLength = 1)]
    string Name,

    [Required, EmailAddress, StringLength(254)]
    string Email,

    [Required, StringLength(72, MinimumLength = 8)]
    string Password);
