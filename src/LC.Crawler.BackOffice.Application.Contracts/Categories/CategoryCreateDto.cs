using LC.Crawler.BackOffice.Enums;
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
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public CategoryType CategoryType { get; set; } = ((CategoryType[])Enum.GetValues(typeof(CategoryType)))[0];
        public Guid? ParentCategoryId { get; set; }
    }
}