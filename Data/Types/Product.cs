public class Product
{
  public required string Id { get; set; }
  public required string Title { get; set; }
  public string? Description { get; set; }
  public required double Price { get; set; }
  public int Sale { get; set; }
  public int Stock { get; set; }
  public required SubCategory SubCategory { get; set; }
  public required string SubCategoryId { get; set; }
}
