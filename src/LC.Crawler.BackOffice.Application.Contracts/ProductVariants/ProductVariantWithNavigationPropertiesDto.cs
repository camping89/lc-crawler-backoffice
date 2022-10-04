using LC.Crawler.BackOffice.Products;

using System;
using Volo.Abp.Application.Dtos;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.ProductVariants
{
    public class ProductVariantWithNavigationPropertiesDto
    {
        public ProductVariantDto ProductVariant { get; set; }

        public ProductDto Product { get; set; }

    }
}