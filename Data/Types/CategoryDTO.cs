﻿namespace net_backend.Data.Types
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public byte[]? Image { get; set; }
        public List<SubCategoryDTO>? SubCategories { get; set; }
    }
}
