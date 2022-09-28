using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.CrawlerAccounts
{
    public interface ICrawlerAccountsAppService : IApplicationService
    {
        Task<PagedResultDto<CrawlerAccountDto>> GetListAsync(GetCrawlerAccountsInput input);

        Task<CrawlerAccountDto> GetAsync(Guid id);

        Task DeleteAsync(Guid id);

        Task<CrawlerAccountDto> CreateAsync(CrawlerAccountCreateDto input);

        Task<CrawlerAccountDto> UpdateAsync(Guid id, CrawlerAccountUpdateDto input);
    }
}