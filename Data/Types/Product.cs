namespace net_backend.Data.Types;
public class Product
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public byte? Sale { get; set; }
    public byte Stock { get; set; }
    public int SubCategoryId { get; set; }
    public SubCategory? SubCategory { get; set; }
}
