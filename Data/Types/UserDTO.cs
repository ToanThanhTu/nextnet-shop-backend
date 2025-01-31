
namespace net_backend.Data.Types
{
    public class UserDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public List<OrderDTO>? Orders { get; set; }
        public List<CartItemDTO>? CartItems { get; set; }
    }
}
