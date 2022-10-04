using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.ProductReviews;
using LC.Crawler.BackOffice.Products;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.Publics;

public interface IProductPublicAppService : IApplicationService
{
    Task<PagedResultDto<ProductWithNavigationPropertiesResultDto>> GetListAsync(GetProductsInput input);
    Task<ProductCommentsResultDto> GetProductCommentsAsync(Guid productId);
    Task<ProductReviewsResultDto> GetProductReviewsAsync(Guid productId);
}