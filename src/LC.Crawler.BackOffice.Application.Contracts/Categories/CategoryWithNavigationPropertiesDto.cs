using LC.Crawler.BackOffice.Categories;

using System;
using Volo.Abp.Application.Dtos;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.Categories
{
    public class CategoryWithNavigationPropertiesDto
    {
        public CategoryDto Category { get; set; }

        public CategoryDto Category1 { get; set; }

    }
}