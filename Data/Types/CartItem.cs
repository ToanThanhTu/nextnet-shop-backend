using System.ComponentModel.DataAnnotations.Schema;

namespace net_backend.Data.Types
{
    public class CartItem
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("user")]
        public User? User { get; set; }

        [Column("product")]
        public Product? Product { get; set; }
    }
}
