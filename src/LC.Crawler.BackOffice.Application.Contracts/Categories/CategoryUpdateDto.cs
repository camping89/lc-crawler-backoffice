using LC.Crawler.BackOffice.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.Categories
{
    public class CategoryUpdateDto : IHasConcurrencyStamp
    {
        [Required]
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public CategoryType CategoryType { get; set; }
        public Guid? ParentCategoryId { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}