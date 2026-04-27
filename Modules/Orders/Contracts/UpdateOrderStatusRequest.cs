using System.ComponentModel.DataAnnotations;

namespace net_backend.Modules.Orders.Contracts;

public record UpdateOrderStatusRequest(
    [Required, StringLength(50)] string Status);
