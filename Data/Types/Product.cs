using System.ComponentModel.DataAnnotations.Schema;

namespace net_backend.Data.Types;

public class Product
{
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public required string Title { get; set; }

    [Column("slug")]
    public string? Slug { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [Column("sale")]
    public byte Sale { get; set; }

    [Column("sale_price")]
    public decimal SalePrice { get; set; }

    [Column("stock")]
    public byte Stock { get; set; }

    [Column("sold")]
    public int? Sold { get; set; }

    [Column("image")]
    public byte[]? Image { get; set; }

    [Column("subcategory_id")]
    public int SubCategoryId { get; set; }

    [Column("sub_category")]
    public SubCategory? SubCategory { get; set; }
}
