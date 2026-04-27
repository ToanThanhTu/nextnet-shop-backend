using System.ComponentModel.DataAnnotations;

namespace net_backend.Modules.Users.Contracts;

public record LoginRequest(
    [Required, EmailAddress, StringLength(254)]
    string Email,

    [Required, StringLength(72, MinimumLength = 1)]
    string Password);
