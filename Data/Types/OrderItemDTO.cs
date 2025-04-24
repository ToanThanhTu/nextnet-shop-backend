using System.ComponentModel.DataAnnotations.Schema;

namespace net_backend.Data.Types
{
    public class OrderItemDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("product")]
        public Product? Product { get; set; }
    }
}
