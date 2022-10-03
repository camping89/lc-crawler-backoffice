using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.Categories
{
    public class CategoryCreateDto
    {
        [Required]
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
    }
}