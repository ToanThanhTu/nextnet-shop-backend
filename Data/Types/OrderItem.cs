using System.ComponentModel.DataAnnotations.Schema;

namespace net_backend.Data.Types
{
    public class OrderItem
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("order")]
        public Order? Order { get; set; }

        [Column("product")]
        public Product? Product { get; set; }
    }
}
