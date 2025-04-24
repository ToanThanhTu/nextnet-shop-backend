
using System.ComponentModel.DataAnnotations.Schema;

namespace net_backend.Data.Types
{
    public class UserDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public required string Name { get; set; }

        [Column("email")]
        public required string Email { get; set; }

        [Column("role")]
        public required string Role { get; set; }

        [Column("orders")]
        public List<OrderDTO>? Orders { get; set; }

        [Column("cart_items")]
        public List<CartItemDTO>? CartItems { get; set; }
    }
}
