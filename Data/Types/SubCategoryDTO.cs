﻿namespace net_backend.Data.Types
{
    public class SubCategoryDTO
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public List<ProductDTO>? Products { get; set; }
    }
}