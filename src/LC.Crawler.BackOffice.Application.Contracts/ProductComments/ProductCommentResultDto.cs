using System;
using System.Collections.Generic;
using LC.Crawler.BackOffice.Products;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.ProductComments;

public class ProductCommentResultDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public string Content { get; set; }
    public int Likes { get; set; }
    public DateTime? CreatedAt { get; set; }

}

public class ProductCommentsResultDto
{
    public string DataSource { get; set; }
    public ProductResultDto Product { get; set; }
    public List<ProductCommentResultDto> Comments { get; set; }
}