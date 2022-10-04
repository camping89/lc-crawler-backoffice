using LC.Crawler.BackOffice.Articles;

using System;
using Volo.Abp.Application.Dtos;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.ArticleComments
{
    public class ArticleCommentWithNavigationPropertiesDto
    {
        public ArticleCommentDto ArticleComment { get; set; }

        public ArticleDto Article { get; set; }

    }
}