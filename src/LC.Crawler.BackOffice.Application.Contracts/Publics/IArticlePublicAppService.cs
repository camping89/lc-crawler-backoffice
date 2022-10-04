using System.Threading.Tasks;
using LC.Crawler.BackOffice.Articles;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.Publics;

public interface IArticlePublicAppService : IApplicationService
{
    Task<PagedResultDto<ArticleWithNavigationPropertiesResultDto>> GetListAsync(GetArticlesInput input);
}