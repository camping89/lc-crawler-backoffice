using LC.Crawler.BackOffice.Products;

using System;
using Volo.Abp.Application.Dtos;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.ProductReviews
{
    public class ProductReviewWithNavigationPropertiesDto
    {
        public ProductReviewDto ProductReview { get; set; }

        public ProductDto Product { get; set; }

    }
}