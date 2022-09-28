using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.CrawlerProxies
{
    public interface ICrawlerProxiesAppService : IApplicationService
    {
        Task<PagedResultDto<CrawlerProxyDto>> GetListAsync(GetCrawlerProxiesInput input);

        Task<CrawlerProxyDto> GetAsync(Guid id);

        Task DeleteAsync(Guid id);

        Task<CrawlerProxyDto> CreateAsync(CrawlerProxyCreateDto input);

        Task<CrawlerProxyDto> UpdateAsync(Guid id, CrawlerProxyUpdateDto input);
    }
}