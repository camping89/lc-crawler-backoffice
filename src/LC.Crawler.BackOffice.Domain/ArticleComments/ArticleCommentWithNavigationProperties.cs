using LC.Crawler.BackOffice.Articles;

using System;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.ArticleComments
{
    public class ArticleCommentWithNavigationProperties
    {
        public ArticleComment ArticleComment { get; set; }

        public Article Article { get; set; }
        

        
    }
}