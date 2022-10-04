using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.ArticleComments;
using LC.Crawler.BackOffice.Articles;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.Publics;

public interface IArticlePublicAppService : IApplicationService
{
    Task<PagedResultDto<ArticleWithNavigationPropertiesResultDto>> GetListAsync(GetArticlesInput input);
    Task<ArticleCommentsResultDto> GetArticleCommentsAsync(Guid articleId);
}