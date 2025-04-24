using System.ComponentModel.DataAnnotations.Schema;

namespace net_backend.Data.Types;

public class SubCategory
{
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public required string Title { get; set; }

    [Column("slug")]
    public string? Slug { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("image")]
    public byte[]? Image { get; set; }

    [Column("category")]
    public Category? Category { get; set; }

    [Column("products")]
    public List<Product>? Products { get; set; }
}