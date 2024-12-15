public class SubCategory
{
  public required string Id { get; set; }
  public required string Title { get; set; }
  public string? Description { get; set; }
  public required string CategoryId { get; set; }
  public required Category Category { get; set; }
  public List<Product> Products { get; set; } = [];
}
