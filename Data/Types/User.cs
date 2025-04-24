using System.ComponentModel.DataAnnotations.Schema;

namespace net_backend.Data.Types
{
    public class User
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public required string Name { get; set; }

        [Column("email")]
        public required string Email { get; set; }

        [Column("password_hash")]
        public required string PasswordHash { get; set; }

        [Column("role")]
        public required string Role { get; set; }

        [Column("orders")]
        public List<Order>? Orders { get; set; }

        [Column("cart_items")]
        public List<CartItem>? CartItems { get; set; }
    }
}
