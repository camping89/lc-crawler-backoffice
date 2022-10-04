using System.Threading.Tasks;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Products;
using LC.Crawler.BackOffice.Publics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.Controllers.Publics;

[AllowAnonymous]
[ControllerName("Public")]
[Route("api/app-public")]
[ApiExplorerSettings(GroupName = "v1-public")]
public class PublicController : BackOfficeController, IArticlePublicAppService,IProductPublicAppService
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
    [Route("products")]
    public Task<PagedResultDto<ProductWithNavigationPropertiesResultDto>> GetListAsync(GetProductsInput input)
    {
        return _productPublicAppService.GetListAsync(input);
    }
}