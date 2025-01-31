namespace net_backend.Data.Types
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalPrice { get; set; }
        public required string Status { get; set; }
        public List<OrderItemDTO>? OrderItems { get; set; }
    }
}
