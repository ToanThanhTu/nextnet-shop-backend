using System.ComponentModel.DataAnnotations.Schema;

namespace net_backend.Data.Types
{
    public class SubCategoryDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public required string Title { get; set; }

        [Column("slug")]
        public string? Slug { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("image")]
        public byte[]? Image { get; set; }

        [Column("products")]
        public List<Product>? Products { get; set; }
    }
}
