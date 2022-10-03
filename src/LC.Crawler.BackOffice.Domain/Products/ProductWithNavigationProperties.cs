using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Medias;

using System;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.Products
{
    public class ProductWithNavigationProperties
    {
        public Product Product { get; set; }

        

        public List<Category> Categories { get; set; }
        public List<Media> Medias { get; set; }
        
    }
}