using System.ComponentModel.DataAnnotations.Schema;

namespace net_backend.Data.Types
{
    public class Order
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column("total_price")]
        public decimal TotalPrice { get; set; }

        [Column("status")]
        public string Status { get; set; } = "Pending";

        [Column("user")]
        public User? User { get; set; }

        [Column("order_items")]
        public List<OrderItem>? OrderItems { get; set; }
    }
}
