namespace net_backend.Data.Types
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending";
        public User? User { get; set; }
        public List<OrderItem>? OrderItems { get; set; }
    }
}
