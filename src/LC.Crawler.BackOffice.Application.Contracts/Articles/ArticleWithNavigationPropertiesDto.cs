using LC.Crawler.BackOffice.Categories;

using System;
using Volo.Abp.Application.Dtos;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.Articles
{
    public class ArticleWithNavigationPropertiesDto
    {
        public ArticleDto Article { get; set; }

        public List<CategoryDto> Categories { get; set; }

    }
}