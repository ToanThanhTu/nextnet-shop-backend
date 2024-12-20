public class Product
{
  public int Id { get; set; }
  public required string Title { get; set; }
  public string? Description { get; set; }
  public double Price { get; set; }
  public int? Sale { get; set; }
  public int Stock { get; set; }
  public int SubCategoryId { get; set; }
  public SubCategory? SubCategory { get; set; }
}
