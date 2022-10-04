using LC.Crawler.BackOffice.Shared;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.Articles
{
    public interface IArticlesAppService : IApplicationService
    {
        Task<PagedResultDto<ArticleWithNavigationPropertiesDto>> GetListAsync(GetArticlesInput input);

        Task<ArticleWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id);

        Task<ArticleDto> GetAsync(Guid id);

        Task<PagedResultDto<LookupDto<Guid?>>> GetMediaLookupAsync(LookupRequestDto input);

        Task<PagedResultDto<LookupDto<Guid>>> GetDataSourceLookupAsync(LookupRequestDto input);

        Task<PagedResultDto<LookupDto<Guid>>> GetCategoryLookupAsync(LookupRequestDto input);

        Task DeleteAsync(Guid id);

        Task<ArticleDto> CreateAsync(ArticleCreateDto input);

        Task<ArticleDto> UpdateAsync(Guid id, ArticleUpdateDto input);
    }
}