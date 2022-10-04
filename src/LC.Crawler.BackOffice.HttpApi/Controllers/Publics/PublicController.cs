using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.ArticleComments;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.ProductReviews;
using LC.Crawler.BackOffice.Products;
using LC.Crawler.BackOffice.Publics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.Controllers.Publics;

[AllowAnonymous]
[ControllerName("Public")]
[Route("api/public")]
[ApiExplorerSettings(GroupName = "v1-public")]
public class PublicController : BackOfficeController, IArticlePublicAppService, IProductPublicAppService
{
    private readonly IArticlePublicAppService _articlePublicAppService;
    private readonly IProductPublicAppService _productPublicAppService;

    public PublicController(IArticlePublicAppService articlePublicAppService, IProductPublicAppService productPublicAppService)
    {
        _articlePublicAppService = articlePublicAppService;
        _productPublicAppService = productPublicAppService;
    }

    [HttpGet]
    [Route("articles")]
    public Task<PagedResultDto<ArticleWithNavigationPropertiesResultDto>> GetListAsync(GetArticlesInput input)
    {
        return _articlePublicAppService.GetListAsync(input);
    }
    
    [HttpGet]
    [Route("article/{articleId}/comments")]
    public Task<ArticleCommentsResultDto> GetArticleCommentsAsync(Guid articleId)
    {
        return _articlePublicAppService.GetArticleCommentsAsync(articleId);
    }
    
    [HttpGet]
    [Route("product/{productId}/reviews")]
    public Task<ProductReviewsResultDto> GetProductReviewsAsync(Guid productId)
    {
        return _productPublicAppService.GetProductReviewsAsync(productId);
    }

    [HttpGet]
    [Route("products")]
    public Task<PagedResultDto<ProductWithNavigationPropertiesResultDto>> GetListAsync(GetProductsInput input)
    {
        return _productPublicAppService.GetListAsync(input);
    }
    
    [HttpGet]
    [Route("product/{productId}/comments")]
    public Task<ProductCommentsResultDto> GetProductCommentsAsync(Guid productId)
    {
        return _productPublicAppService.GetProductCommentsAsync(productId);
    }
}