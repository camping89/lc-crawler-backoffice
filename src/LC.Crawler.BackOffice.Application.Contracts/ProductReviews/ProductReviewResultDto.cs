using System;
using System.Collections.Generic;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.Products;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.ProductReviews;

public class ProductReviewResultDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public string Content { get; set; }
    public decimal Rating { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int Likes { get; set; }

}

public class ProductReviewsResultDto
{
    public string DataSource { get; set; }
    public ProductResultDto Product { get; set; }
    public List<ProductReviewResultDto> Reviews { get; set; }
}