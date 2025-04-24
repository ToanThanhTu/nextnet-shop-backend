using System.ComponentModel.DataAnnotations.Schema;

namespace net_backend.Data.Types
{
    public class CategoryDTO
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

        [Column("sub_categories")]
        public List<SubCategoryDTO>? SubCategories { get; set; }
    }
}
