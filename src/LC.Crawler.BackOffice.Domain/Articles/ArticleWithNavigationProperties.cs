using LC.Crawler.BackOffice.Categories;

using System;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.Articles
{
    public class ArticleWithNavigationProperties
    {
        public Article Article { get; set; }

        

        public List<Category> Categories { get; set; }
        
    }
}