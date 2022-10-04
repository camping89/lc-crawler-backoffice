using LC.Crawler.BackOffice.Products;

using System;
using Volo.Abp.Application.Dtos;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.ProductComments
{
    public class ProductCommentWithNavigationPropertiesDto
    {
        public ProductCommentDto ProductComment { get; set; }

        public ProductDto Product { get; set; }

    }
}