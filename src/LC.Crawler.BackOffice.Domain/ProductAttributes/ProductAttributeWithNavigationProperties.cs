using LC.Crawler.BackOffice.Products;

using System;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.ProductAttributes
{
    public class ProductAttributeWithNavigationProperties
    {
        public ProductAttribute ProductAttribute { get; set; }

        public Product Product { get; set; }
        

        
    }
}