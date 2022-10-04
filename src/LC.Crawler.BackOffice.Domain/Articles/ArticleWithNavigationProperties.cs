using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Medias;

using System;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.Articles
{
    public class ArticleWithNavigationProperties
    {
        public Article Article { get; set; }

        public Media Media { get; set; }
        public DataSource DataSource { get; set; }
        

        public List<Category> Categories { get; set; }
        public List<Media> Medias { get; set; }
        
    }
}