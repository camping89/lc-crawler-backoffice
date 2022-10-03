using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Medias;

using System;
using Volo.Abp.Application.Dtos;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.Products
{
    public class ProductWithNavigationPropertiesDto
    {
        public ProductDto Product { get; set; }

        public List<CategoryDto> Categories { get; set; }
        public List<MediaDto> Medias { get; set; }

    }
}