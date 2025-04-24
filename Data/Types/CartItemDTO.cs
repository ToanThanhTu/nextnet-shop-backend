using System.ComponentModel.DataAnnotations.Schema;

namespace net_backend.Data.Types
{
    public class CartItemDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("product")]
        public Product? Product { get; set; }
    }
}
