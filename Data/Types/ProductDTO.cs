namespace net_backend.Data.Types
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public byte Sale { get; set; }
        public decimal SalePrice { get; set; }
        public byte Stock { get; set; }
        public int? Sold { get; set; }
        public int SubCategoryId { get; set; }
    }
}
