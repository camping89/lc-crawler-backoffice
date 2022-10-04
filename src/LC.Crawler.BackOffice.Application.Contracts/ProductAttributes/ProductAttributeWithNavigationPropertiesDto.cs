using LC.Crawler.BackOffice.Products;

using System;
using Volo.Abp.Application.Dtos;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.ProductAttributes
{
    public class ProductAttributeWithNavigationPropertiesDto
    {
        public ProductAttributeDto ProductAttribute { get; set; }

        public ProductDto Product { get; set; }

    }
}