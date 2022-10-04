using LC.Crawler.BackOffice.Products;

using System;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.ProductVariants
{
    public class ProductVariantWithNavigationProperties
    {
        public ProductVariant ProductVariant { get; set; }

        public Product Product { get; set; }
        

        
    }
}