public class Category
{
  public required string Id { get; set; }
  public required string Title { get; set; }
  public string? Description { get; set; }
  public List<SubCategory> SubCategories { get; set; } = [];
}
