using LC.Crawler.BackOffice.Shared;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.ArticleComments
{
    public interface IArticleCommentsAppService : IApplicationService
    {
        Task<PagedResultDto<ArticleCommentWithNavigationPropertiesDto>> GetListAsync(GetArticleCommentsInput input);

        Task<ArticleCommentWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id);

        Task<ArticleCommentDto> GetAsync(Guid id);

        Task<PagedResultDto<LookupDto<Guid>>> GetArticleLookupAsync(LookupRequestDto input);

        Task DeleteAsync(Guid id);

        Task<ArticleCommentDto> CreateAsync(ArticleCommentCreateDto input);

        Task<ArticleCommentDto> UpdateAsync(Guid id, ArticleCommentUpdateDto input);
    }
}