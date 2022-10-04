using LC.Crawler.BackOffice.Products;

using System;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.ProductComments
{
    public class ProductCommentWithNavigationProperties
    {
        public ProductComment ProductComment { get; set; }

        public Product Product { get; set; }
        

        
    }
}