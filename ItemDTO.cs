public class ItemDTO
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public double Price { get; set; }
  public bool IsSold { get; set; }

  public ItemDTO() { }
  public ItemDTO(Item item) =>
    (Id, Name, Price, IsSold) = (item.Id, item.Name, item.Price, item.IsSold);
}
