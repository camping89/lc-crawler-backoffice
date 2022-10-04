using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Medias;

using System;
using Volo.Abp.Application.Dtos;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.Articles
{
    public class ArticleWithNavigationPropertiesDto
    {
        public ArticleDto Article { get; set; }

        public MediaDto Media { get; set; }
        public DataSourceDto DataSource { get; set; }
        public List<CategoryDto> Categories { get; set; }
        public List<MediaDto> Medias { get; set; }

    }
}