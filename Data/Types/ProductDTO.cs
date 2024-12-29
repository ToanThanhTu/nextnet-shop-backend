namespace net_backend.Data.Types
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public byte? Sale { get; set; }
        public byte Stock { get; set; }
    }
}
