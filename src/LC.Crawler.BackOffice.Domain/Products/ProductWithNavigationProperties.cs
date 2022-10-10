using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Medias;

using System;
using System.Collections.Generic;
using LC.Crawler.BackOffice.ProductAttributes;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.ProductReviews;
using LC.Crawler.BackOffice.ProductVariants;

namespace LC.Crawler.BackOffice.Products
{
    public class ProductWithNavigationProperties
    {
        public Product Product { get; set; }

        public Media Media { get; set; }
        public DataSource DataSource { get; set; }
        

        public List<Category> Categories { get; set; }
        public List<Media> Medias { get; set; }
        
        
        public List<ProductAttribute> Attributes { get; set; }
        public List<ProductVariant> Variants { get; set; }
        
        public List<ProductComment> Comments { get; set; }
        public List<ProductReview> Reviews { get; set; }
        
    }
}