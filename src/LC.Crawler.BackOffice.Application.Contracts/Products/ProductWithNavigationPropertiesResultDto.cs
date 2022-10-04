using System;
using System.Collections.Generic;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.ProductAttributes;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.ProductReviews;
using LC.Crawler.BackOffice.ProductVariants;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.Products;

public class ProductWithNavigationPropertiesResultDto
{
    public string DataSource { get; set; }
    public ProductResultDto Product { get; set; }
    public List<CategoryResultDto> Categories { get; set; }
    public List<MediaResultDto> Images { get; set; }
    public List<ProductVariantResultDto> Variants { get; set; }
    public List<ProductAttributeResultDto> Attributes { get; set; }
    // public List<ProductCommentResultDto> Comments { get; set; }
    // public List<ProductReviewResultDto> Reviews { get; set; }
}

public class ProductResultDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string ShortDescription { get; set; }
    public string Description { get; set; }
    public string FeatureImageUrl { get; set; }
}

