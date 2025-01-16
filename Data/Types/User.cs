namespace net_backend.Data.Types
{
    public class User
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required string Role { get; set; }
        public List<Order> Orders { get; set; } = [];
        public List<CartItem> CartItems { get; set; } = [];
    }
}
