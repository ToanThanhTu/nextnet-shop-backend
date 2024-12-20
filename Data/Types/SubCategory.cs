public class SubCategory
{
  public int Id { get; set; }
  public required string Title { get; set; }
  public string? Description { get; set; }
  public int CategoryId { get; set; }
  public Category? Category { get; set; }
  public List<Product>? Products { get; set; }
}
